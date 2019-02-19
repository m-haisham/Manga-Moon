using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using System.IO;

namespace Mago
{
    public class ReaderViewModel : BaseViewModel
    {
        private ObservableCollection<PageViewModel> _pages;
        private ObservableCollection<string> _chapterNames;
        private ObservableCollection<int> _zoom;
        private ObservableCollection<int> _margin;

        private string _mangaName;

        private bool _isTopBarOpen;
        private bool _nextChapterExists;
        private bool _lastChapterExists;
        private bool _ScrollTop;

        private int _selectedZoomIndex;
        private int _selectedMarginIndex;
        private int? _selectedChapterIndex = null;

        public ICommand LoadPrevious { get; set; }
        public ICommand LoadNext { get; set; }

        private string URL;
        private MainViewModel MainViewModel;
        private List<ChapterHolder> _chapters;

        TokenPool tokenPool = new TokenPool(5);
        Task lastDownloadTask;
        Task currentDownloadTask;
        
        public ReaderViewModel(MainViewModel MainView)
        {
            _pages = new ObservableCollection<PageViewModel>();
            _zoom = new ObservableCollection<int> { 25, 50, 75, 100, 125, 150, 175, 200, 250, 300 };
            _margin = new ObservableCollection<int> { 0, 5 };
            SelectedMarginIndex = 1;
            SelectedZoomIndex = 3;
            LoadNext = new RelayCommand(AdvanceSelected);
            LoadPrevious = new RelayCommand(BackSelected);
            MainViewModel = MainView;
        }

        public async Task Setup(int index)
        {
            //Split the tasks
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(URL);
            
            //Get Chapters
            ObservableCollection<ChapterInfo> chapters = new ObservableCollection<ChapterInfo>();

            IEnumerable<HtmlNode> chapterCollection = doc.DocumentNode.SelectNodes("//div[@class='chapter-list']").Descendants("div");

            ChapterInfo newChapter;
            foreach (var node in chapterCollection)
            {
                newChapter = new ChapterInfo();
                HtmlNode link = node.Descendants("span").First().Descendants("a").First();
                newChapter.href = link.Attributes["href"].Value;
                newChapter.Name = link.InnerText;

                chapters.Add(newChapter);
            }
            chapters = new ObservableCollection<ChapterInfo>(chapters.Reverse());

            //Call and Wait till main thread updates collection
            await Application.Current.Dispatcher.Invoke(async () => 
            {
                await SetChapterData(chapters);

                //Clear Collection
                _pages.Clear();

                //Set Buttons
                CalculateChapterInfo();
                
                //Start download of chapter by setting chapter index
                SelectedChapterIndex = index;
            });
        }

        public async Task LoadChapterAsync(int index, CancellationToken token)
        {
            //wait till previous task is cancelled
            while (lastDownloadTask != null && lastDownloadTask.Status == TaskStatus.WaitingForActivation && !lastDownloadTask.IsCanceled)
            {
                Debug.WriteLine("Waiting for last task to cancel");

                //wait for 25ms
                await Task.Delay(25);
            }

            //get current selected chapters url
            string currentUrl = _chapters[index].url;

            //get page paths
            List<string> pagePaths = await GetPagePaths(currentUrl);

            //Itereate through pages
            for (int i = 0; i < pagePaths.Count; i++)
            {
                BitmapImage imageSource;

                //if chapter already downloaded
                if(_chapters[(int)SelectedChapterIndex].imageTempPaths.Count >= i + 1 && _chapters[(int)SelectedChapterIndex].imageTempPaths[i] != null)
                {
                    //load image from byte array on file
                    imageSource = LoadBitmapFromArrayFile(_chapters[(int)SelectedChapterIndex].imageTempPaths[i]);
                }
                else
                {
                    //Download the data to byte array
                    byte[] byteArray = DataConversionHelper.DownloadToArray(pagePaths[i]);

                    //save byte array to temporary file
                    string tempPath = SaveByteArrayToTempFile(byteArray);

                    //record temporary path
                    _chapters[(int)SelectedChapterIndex].imageTempPaths.Add(tempPath);
                    
                    //convert array to Bitmap
                    imageSource = byteArray.ToFreezedBitmapImage();
                }
                
                //check for cancellation before applying image
                token.ThrowIfCancellationRequested();
                
                //Call main thread to add to collection
                Application.Current.Dispatcher.Invoke(() => { AddToReaderAsync(imageSource); });
            }

            //Rise notification
            Application.Current.Dispatcher.Invoke(() => MainViewModel.NotificationsViewModel.AddNotification("Current chapter fully loaded", NotificationMode.Success));

            #region Download next chapter to temporary path

            //if next chapter doesnt exist exit method
            if (!NextChapterExists) return;

            //Next chapters url
            string NextUrl = _chapters[index + 1].url;

            //get page paths
            pagePaths = await GetPagePaths(NextUrl);

            //Itereate through pages
            for (int i = 0; i < pagePaths.Count; i++)
            {
                //check for cancellation before downloading image
                token.ThrowIfCancellationRequested();
                
                //if page of chapter havent been downloaded
                if (!(_chapters[(int)SelectedChapterIndex + 1].imageTempPaths.Count >= i + 1 && _chapters[(int)SelectedChapterIndex + 1].imageTempPaths[i] != null))
                {
                    //Download the data to byte array
                    byte[] byteArray = DataConversionHelper.DownloadToArray(pagePaths[i]);

                    //save byte array to temporary file
                    string tempPath = SaveByteArrayToTempFile(byteArray);

                    //record temporary path
                    _chapters[(int)SelectedChapterIndex + 1].imageTempPaths.Add(tempPath);
                }
                
            }

            #endregion
        }

        public async Task SetChapterData(ObservableCollection<ChapterInfo> infoList)
        {
            ChapterNames = new ObservableCollection<string>();
            _chapters = new List<ChapterHolder>();
            for (int i = 0; i < infoList.Count; i++)
            {
                ChapterInfo info = infoList[i];

                //Add to chapter list Combobox
                ChapterNames.Add(info.Name);

                //Add to chapter holder
                _chapters.Add(
                    new ChapterHolder
                    {
                        url = info.href
                    });

                //await Task.Delay((int)(0.001 * 1000));
            }
            
        }

        public async Task<List<string>> GetPagePaths(string ChapterPath)
        {
            //get all links of pages
            List<string> pagePaths = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(ChapterPath);

            //get division with id "vungdoc"
            HtmlNode idNode = doc.GetElementbyId("vungdoc");

            //get all img nodes
            IEnumerable<HtmlNode> RawPageList = idNode.Descendants("img");
            for (int i = 0; i < RawPageList.Count(); i++)
            {
                HtmlNode pageNode = RawPageList.ElementAt(i);

                //get value of attribute source
                string url = pageNode.Attributes["src"].Value;

                //add value of type url to page list
                pagePaths.Add(url);
            }

            //return paths
            return pagePaths;
        }

        public void AddToReaderAsync(BitmapImage source)
        {
            //Create new Page view model
            PageViewModel pgm = new PageViewModel(source)
            {
                //Apply Zoom && Margin
                Zoom = (float)_zoom[SelectedZoomIndex] / 100f,
                Margin = Margin[SelectedMarginIndex]
            };

            //add to Collection
            _pages.Add(pgm);
        }

        public string SaveByteArrayToTempFile(byte[] array)
        {
            //get temporary file path
            string tempPath = Path.GetTempFileName();

            //create temporary file
            FileStream stream = new FileStream(tempPath, FileMode.Create);

            //write byteArray to file to file
            stream.Write(array, 0, array.Count());

            //close filestream
            stream.Close();

            //return file path
            return tempPath;
        }

        public BitmapImage LoadBitmapFromArrayFile(string path)
        {
            //read bytes to array
            byte[] array = File.ReadAllBytes(path);

            //convert array to Bitmap
            BitmapImage image = array.ToFreezedBitmapImage();

            //return Bitmap
            return image;
        }
        
        public void ClearTemporary()
        {
            //for all chapters
            for (int i = 0; i < _chapters?.Count; i++)
            {
                //for all pages in chapter
                for (int j = 0; j < _chapters[i].imageTempPaths?.Count; j++)
                {
                    //delete temporary file
                    File.Delete(_chapters[i].imageTempPaths[j]);
                }
            }

            Debug.WriteLine("All temporary files deleted");

        }
        
        public void SetURL(string url)
        {
            URL = url;
        }

        public void AdvanceSelected()
        {
            SelectedChapterIndex += 1;
        }

        public void BackSelected()
        {
            SelectedChapterIndex -= 1;
        }

        public void CalculateChapterInfo()
        {
            NextChapterExists = SelectedChapterIndex < _chapters.Count - 1;
            LastChapterExists = SelectedChapterIndex > 0;
        }
        
        public void ApplyZoom()
        {
            foreach (var page in Pages)
            {
                page.Zoom = (float)_zoom[_selectedZoomIndex] / 100f;
            }
        }
        public void ApplyMargin()
        {
            foreach (var page in Pages)
            {
                page.Margin = _margin[SelectedMarginIndex];
            }
        }

        public ObservableCollection<PageViewModel> Pages
        {
            get => _pages;
            set
            {
                if (_pages == value) return;
                _pages = value;
            }
        }
        public ObservableCollection<string> ChapterNames
        {
            get => _chapterNames;
            set
            {
                if (_chapterNames == value) return;
                _chapterNames = value;
            }
        }
        public ObservableCollection<int> Zoom => _zoom;
        public ObservableCollection<int> Margin => _margin;

        public string MangaName
        {
            get { return _mangaName; }
            set
            {
                if (_mangaName == value) return;
                _mangaName = value;
            }
        }

        public int SelectedZoomIndex
        {
            get { return _selectedZoomIndex; }
            set
            {
                if (_selectedZoomIndex == value) return;
                _selectedZoomIndex = value;
                ApplyZoom();
            }
        }
        public int SelectedMarginIndex
        {
            get { return _selectedMarginIndex; }
            set
            {
                if (_selectedMarginIndex == value) return;
                _selectedMarginIndex = value;
                ApplyMargin();
            }
        }
        public int? SelectedChapterIndex
        {
            get => _selectedChapterIndex;
            set
            {
                if (_selectedChapterIndex == value) return;
                _selectedChapterIndex = value;

                //if value if null, exit
                if (_selectedChapterIndex == null) return;

                //re set the buttons
                CalculateChapterInfo();

                //assign last download task
                lastDownloadTask = currentDownloadTask;

                //get new token
                CancellationToken newToken = tokenPool.getToken();

                //Scroll to top
                ScrollTop = true;
                _ScrollTop = false;

                //clear current pages
                _pages.Clear();

                //assign new download task
                currentDownloadTask = Task.Run(() => LoadChapterAsync((int)_selectedChapterIndex, newToken), newToken);
            }
        }

        public bool IsTopBarOpen
        {
            get { return _isTopBarOpen; }
            set
            {
                if (_isTopBarOpen == value) return;
                _isTopBarOpen = value;
            }
        }
        public bool NextChapterExists
        {
            get { return _nextChapterExists; }
            set
            {
                if (_nextChapterExists == value) return;
                _nextChapterExists = value;
            }
        }
        public bool LastChapterExists
        {
            get { return _lastChapterExists; }
            set
            {
                if (_lastChapterExists == value) return;
                _lastChapterExists = value;
            }
        }
        public bool ScrollTop
        {
            get { return _ScrollTop; }
            set
            {
                _ScrollTop = value;
            }
        }
    }
}
