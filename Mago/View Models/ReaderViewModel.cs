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

namespace Mago
{
    public class ReaderViewModel : BaseViewModel
    {
        private ObservableCollection<PageViewModel> _pages;
        private ObservableCollection<string> _chapterNames;
        private ObservableCollection<int> _zoom;
        private ObservableCollection<int> _margin;

        private bool _isTopBarOpen;
        private bool _nextChapterExists;
        private bool _lastChapterExists;
        private bool _ScrollTop;

        private int _selectedZoomIndex;
        private int _selectedMarginIndex;
        private int _selectedChapterIndex;

        public ICommand LoadPrevious { get; set; }
        public ICommand LoadNext { get; set; }

        private string URL;

        private List<ChapterHolder> _chapters;

        TokenPool tokenPool = new TokenPool(5);
        Task lastDownloadTask;
        Task currentDownloadTask;

        public ReaderViewModel()
        {
            _pages = new ObservableCollection<PageViewModel>();
            _zoom = new ObservableCollection<int> { 25, 50, 75, 100, 125, 150, 175, 200, 250, 300 };
            _margin = new ObservableCollection<int> { 0, 5 };
            SelectedMarginIndex = 1;
            SelectedZoomIndex = 3;
            LoadNext = new RelayCommand(AdvanceSelected);
            LoadPrevious = new RelayCommand(BackSelected);
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
                Debug.WriteLine("Waiting for last task to cancel");

                //wait for 25ms
                await Task.Delay(25);
            
            //get current selected chapters url
            string currentUrl = _chapters[index].url;

            //get all links of pages
            List<string> pagePaths = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(currentUrl);

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

            //Create bitmap images for the links
            for (int i = 0; i < pagePaths.Count; i++)
            {
                //using written helper function
                BitmapImage imageSource = pagePaths[i].ToFreezedBitmapImage();

                //check for cancellation before applying image
                token.ThrowIfCancellationRequested();
                
                //Call main thread to add to collection
                Application.Current.Dispatcher.Invoke(() => { AddToReaderAsync(imageSource); });
            }
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

        public async Task SetChapters()
        {
             _chapters = await URL.ToRawChapters();
            _chapterNames = new ObservableCollection<string>();
            for (int i = 0; i < _chapters.Count; i++)
            {
                var list = _chapters[i].url.Split('/');
                _chapterNames.Add(list[list.Length - 1]);
            }
        }

        public async Task LoadSelected()
        {
            if(_chapters[SelectedChapterIndex].images.Count == 0)
            {
                await _chapters[SelectedChapterIndex].DownloadPages();
            }

            ObservableCollection<PageViewModel> _pages = new ObservableCollection<PageViewModel>();
            
            for (int i = 0; i < _chapters[SelectedChapterIndex].images.Count; i++)
            {
                _pages.Add(new PageViewModel(_chapters[SelectedChapterIndex].images[i])
                {
                    Zoom = _zoom[SelectedZoomIndex],
                    Margin = Margin[SelectedMarginIndex]
                });
                
            }

            for (int i = 0; i < _chapters.Count; i++)
            {
                if(i > (SelectedChapterIndex + 1) && i < (SelectedChapterIndex - 1))
                    _chapters[i].images = new List<System.Windows.Media.Imaging.BitmapImage>();
            }

            if (_chapters[SelectedChapterIndex + 1].images.Count == 0)
            {
                await _chapters[SelectedChapterIndex + 1].DownloadPages();
            }

            if (_chapters[SelectedChapterIndex - 1].images.Count == 0)
            {
                await _chapters[SelectedChapterIndex - 1].DownloadPages();
            }
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
        public int SelectedChapterIndex
        {
            get => _selectedChapterIndex;
            set
            {
                if (_selectedChapterIndex == value) return;
                _selectedChapterIndex = value;

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
                currentDownloadTask = Task.Run(() => LoadChapterAsync(_selectedChapterIndex, newToken), newToken);
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
