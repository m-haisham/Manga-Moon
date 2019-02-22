using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.IO;
using HtmlAgilityPack;

namespace Mago
{
    public class DownloadsPanelViewModel : BaseViewModel
    {
        #region Private Members

        private ObservableCollection<DownloadProgressViewModel> _DownloadsPanel;
        private int _animationIndex = 0;
        private PackIconKind _playPackIcon = PackIconKind.Play;
        private bool _allItemsSelected;
        private Visibility _visible = Visibility.Collapsed;

        private MainViewModel MainView;
        private bool isDownloading = false;

        #endregion

        #region Public Commands

        public ICommand ClearCommand { get; set; }
        public ICommand CleanCommand { get; set; }
        public ICommand TogglePlayCommand { get; set; }

        public ICommand UpQueueCommand { get; set; }
        public ICommand DownQueueCommand { get; set; }

        public ICommand DownloadsPanelAnimation { get; set; }

        #endregion

        #region Constructor
        public DownloadsPanelViewModel(MainViewModel Main)
        {
            _DownloadsPanel = new ObservableCollection<DownloadProgressViewModel>();
            ClearCommand = new RelayCommand(ClearSelected);
            CleanCommand = new RelayCommand(Clean);
            TogglePlayCommand = new RelayCommand(TogglePlay);

            MainView = Main;

            DownloadsPanelAnimation = new RelayCommand(() => IncrementDownloadsPanelIndex());
            
        }
        #endregion

        #region Private Methods
        private void ClearSelected()
        {
            List<DownloadProgressViewModel> Markers = GetSelected();

            foreach (var marker in Markers)
                _DownloadsPanel.Remove(marker);
        }

        private void Clean()
        {
            List<DownloadProgressViewModel> Markers = new List<DownloadProgressViewModel>();
            for (int i = 0; i < _DownloadsPanel.Count; i++)
            {
                if (_DownloadsPanel[i].DownloadState == DownloadState.Completed)
                    Markers.Add(_DownloadsPanel[i]);
            }

            foreach (var marker in Markers)
                _DownloadsPanel.Remove(marker);
        }

        private void TogglePlay()
        {
            if (PlayPackIcon == PackIconKind.Pause)
                PlayPackIcon = PackIconKind.Play;
            else if (PlayPackIcon == PackIconKind.Play)
                PlayPackIcon = PackIconKind.Pause;
        }

        private void QueueUp()
        {
            List<int> Markers = GetSelectedIndex();
            for (int i = 0; i < Markers.Count; i++)
            {
                if (_DownloadsPanel[i].DownloadState != DownloadState.InProgress)
                    Swap(_DownloadsPanel[Markers[i]], _DownloadsPanel[Markers[i] + 1]);
            }
        }

        private void QueueDown()
        {
            List<int> Markers = GetSelectedIndex();
            for (int i = Markers.Count; i >= 0; i--)
            {
                if (_DownloadsPanel[i].DownloadState != DownloadState.InProgress || i == _DownloadsPanel.Count - 1)
                    Swap(_DownloadsPanel[Markers[i]], _DownloadsPanel[Markers[i] + 1]);
            }
        }
        
        private async Task IncrementDownloadsPanelIndex()
        {
            AnimationIndex = AnimationIndex == 0 ? 1 : 0;
            switch (AnimationIndex)
            {
                case 0:
                    await Task.Delay(1000);
                    Visible = Visibility.Collapsed;
                    return;

                case 1:
                    Visible = Visibility.Visible;
                    return;
            }
        }

        #endregion

        #region Helper Methods

        private List<DownloadProgressViewModel> GetSelected()
        {
            List<DownloadProgressViewModel> Selected = new List<DownloadProgressViewModel>();
            for (int i = 0; i < _DownloadsPanel.Count; i++)
            {
                if (_DownloadsPanel[i].IsSelected)
                    Selected.Add(_DownloadsPanel[i]);
            }

            return Selected;
        }

        private List<int> GetSelectedIndex()
        {
            List<int> Selected = new List<int>();
            for (int i = 0; i < _DownloadsPanel.Count; i++)
            {
                if (_DownloadsPanel[i].IsSelected)
                    Selected.Add(i);
            }

            return Selected;
        }

        private void Swap<T>(T element1,T element2)
        {
            var backup = element1;
            element1 = element2;
            element2 = backup;
        }

        private List<byte[]> LoadPaths(List<string> paths)
        {
            List<byte[]> newArray = new List<byte[]>();
            for (int i = 0; i < paths.Count; i++)
            {
                if(File.Exists(paths[i]))
                    newArray.Add(File.ReadAllBytes(paths[i]));
            }
            return newArray;
        } 

        #endregion
        
        private ObservableCollection<DownloadProgressViewModel> Setup()
        {
            var obj = new ObservableCollection<DownloadProgressViewModel>();
            for (int i = 0; i < 10; i++)
            {
                obj.Add(
                    new DownloadProgressViewModel
                    {
                        Header = "Name " + i.ToString(),
                        Description = "desciption",
                        Maximum = 10,
                        DownloadState = DownloadState.Queued
                    }
                );
            }

            return obj;
        }

        public void AddDownload(string url, string chapterName, string mangaName)
        {
            _DownloadsPanel.Add(
                new DownloadProgressViewModel
                {
                    Header = mangaName,
                    Description = chapterName,
                    url = url,
                    Progress = 0,
                    DownloadState = DownloadState.Queued
                });

            if(_DownloadsPanel.Where(n => n.DownloadState == DownloadState.Queued).Count() == 1 && !isDownloading)
            {
                Task.Run(DownloadAsync);
            }
        }

        private async Task DownloadAsync()
        {
            isDownloading = true;

            while(_DownloadsPanel.Count > 0)
            {
                //get first Queued in list
                DownloadProgressViewModel item = null;
                for (int i = 0; i < _DownloadsPanel.Count; i++)
                {
                    if(_DownloadsPanel[i].DownloadState == DownloadState.Queued)
                    {
                        item = _DownloadsPanel[i];
                        break;
                    }
                }

                //if no items queued
                if (item == null) {isDownloading = false; return; }
                
                //if download state is wait
                while (PlayPackIcon == PackIconKind.Play)
                    await Task.Delay(1000);

                //get all page paths of chapter
                List<string> pagePaths = await GetPagePaths(item.url);

                //set progress bar end
                item.Maximum = pagePaths.Count;
                
                //Change download State
                item.DownloadState = DownloadState.InProgress;

                //download all undownloaded files
                for (int i = 0; i < pagePaths.Count; i++)
                {
                    //if download state is wait
                    while (PlayPackIcon == PackIconKind.Play)
                        await Task.Delay(1000);

                    if (!(item.tempPaths.Count >= i + 1 && item.tempPaths[i] != null))
                    {
                        //Download data to a byte array
                        byte[] array = DataConversionHelper.DownloadToArray(pagePaths[i]);

                        //Save to a temporary file
                        string path = SaveByteArrayToTempFile(array);

                        //Remember path
                        item.tempPaths.Add(path);
                    }

                    //advance download progress
                    item.Progress += 1;

                }

                //Create new ch class
                ChSave chSave = new ChSave(LoadPaths(item.tempPaths));

                //create path for download
                string savePath = MainView.Settings.mangaPath + item.Header + "/" + item.Description + ".ch";

                //Save file to path
                SaveSystem.SaveBinary(chSave, savePath);

                //dispose of save class
                chSave = null;

                //Set download status to complete
                item.DownloadState = DownloadState.Completed;

                //notify user according to settings
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (MainView.Settings.chapterDownloadNotifications)
                        MainView.NotificationsViewModel.AddNotification(item.Description + " Download Complete.", NotificationMode.Success);
                    if (MainView.Settings.autoDeleteCompletedDownloads)
                        DownloadsPanel.Remove(item);
                });
                

            }
            
            isDownloading = false;
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

        public ObservableCollection<DownloadProgressViewModel> DownloadsPanel => _DownloadsPanel;

        public bool AllItemsSelected
        {
            get { return _allItemsSelected; }
            set
            {
                if (_allItemsSelected == value) return;

                _allItemsSelected = value;

                foreach (var panel in _DownloadsPanel)
                {
                    panel.IsSelected = value;
                }
            }
        }

        public Visibility Visible
        {
            get { return _visible; }
            set
            {
                if (_visible == value) return;
                _visible = value;
            }
        }
        
        public int AnimationIndex
        {
            get { return _animationIndex; }
            set
            {
                if (_animationIndex == value) return;
                _animationIndex = value;
            }
        }

        public PackIconKind PlayPackIcon
        {
            get { return _playPackIcon; }
            set
            {
                if (_playPackIcon == value) return;
                _playPackIcon = value;
            }
        }
        
    }
}
