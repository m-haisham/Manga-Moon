using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Mago
{
    public class MangaViewModel : BaseViewModel
    {

        public string Url;

        private ObservableCollection<ChapterListItemViewModel> _chapterList;
        private ObservableCollection<GenreItemViewModel> _genreList;
        private ObservableCollection<string> _authorList;

        private string _name;
        private string _description;
        private string _readStatus = "Read";
        private string _mangaStatus = "Ongoing";
        private bool _isRead;
        private bool _isFavourite;
        private bool _isCompleted;
        private BitmapImage _imageSource;

        private byte[] _imageArray;
        private ChapterListItemViewModel[] _SelectedMemory = new ChapterListItemViewModel[2];

        public ICommand DownloadSelected { get; set; }

        public MainViewModel MainView;
        private string mangaSavePath;
        private MgiSave savedinfo;

        public MangaViewModel(MainViewModel mainView)
        {
            MainView = mainView;
            DownloadSelected = new RelayCommand(() => Task.Run(DownloadMultiple));

            GenreList = new ObservableCollection<GenreItemViewModel>();
            AuthorList = new ObservableCollection<string>();
            ChapterList = new ObservableCollection<ChapterListItemViewModel>();
            
        }

        private async Task DownloadMultiple()
        {
            List<int> SelectedIndexList = new List<int>();
            for (int i = 0; i < ChapterList.Count; i++)
            {
                if (ChapterList[i].IsSelected && ChapterList[i].IsNotDownloaded == true)
                    SelectedIndexList.Add(i);
            }
            SelectedIndexList.Reverse();

            foreach (var index in SelectedIndexList)
            {
                AddtoDownloads(ChapterList[index]);
                ChapterList[index].IsNotDownloaded = false;
            }
        }

        public void AddtoDownloads(ChapterListItemViewModel chapter)
        {
            if (mangaSavePath == null)
            {
                SetMangaPath();
                SaveMangaInfo();
            }
            else if(savedinfo?.chapters.Count != ChapterList.Count)
            {
                SaveMangaInfo();
            }
            MainView.DownloadsPanelViewModel.AddDownload(chapter.URL, chapter.Name, Name);
        }

        public async Task OpenInReader(int index)
        {
            MainView.ReaderViewModel.MangaName = Name;
            MainView.ReaderViewModel.SetURL(Url);
            MainView.ReaderViewModel.mangaSavePath = mangaSavePath;

            await MainView.ReaderViewModel.Setup(_chapterList.Count - index - 1);

            MainView.MenuViewModel.TransitionIndex = MainView.MenuViewModel.page.MangaReader;
        }

        #region Helper Methods

        public void SetImageArray(string url)
        {
            WebClient wb = new WebClient();
            _imageArray = wb.DownloadData(url);
        }

        public byte[] GetImageArray(string url)
        {
            WebClient wb = new WebClient();
            return wb.DownloadData(url);
        }

        public void SetMangaPath()
        {
            mangaSavePath = MainView.Settings.mangaPath + Name + "/" + Name + ".mgi";
            if(File.Exists(mangaSavePath))
                savedinfo = SaveSystem.LoadBinary<MgiSave>(mangaSavePath);
        }

        public void SaveMangaInfo()
        {
            MgiSave instance = new MgiSave
            {
                url = Url,
                name = _name,
                description = _description,
                image = ImageSource.ToByteArray(),
                isFavourite = _isFavourite,
                isCompleted = _isCompleted,
                authors = _authorList
            };
            

            for (int i = 0; i < _genreList.Count; i++)
                instance.genres.Add(_genreList[i].Text);

            for (int i = 0; i < _chapterList.Count; i++)
            {
                instance.chapters.Add(_chapterList[i].Name);
                instance.chapterUrls.Add(_chapterList[i].URL);
            }

            SaveSystem.SaveBinary(instance, mangaSavePath);
        }

        public void ImportMangaInfo()
        {
            Url = savedinfo.url;
            Name = savedinfo.name;
            Description = savedinfo.description;
            ImageSource = savedinfo.image?.ToFreezedBitmapImage();
            IsFavourite = savedinfo.isFavourite;
            IsCompleted = savedinfo.isCompleted;

            Dispatcher dispatcher;
            if (Dispatcher.CurrentDispatcher != Application.Current.Dispatcher)
                dispatcher = Application.Current.Dispatcher;
            else
                dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.Invoke(() =>
            {
                AuthorList = savedinfo.authors;

                GenreList.Clear();
                for (int i = 0; i < savedinfo.genres.Count; i++)
                {
                    GenreList.Add(new GenreItemViewModel { Text = savedinfo.genres[i] });
                }

                ChapterList.Clear();
                for (int i = 0; i < savedinfo.chapters.Count; i++)
                {
                    ChapterListItemViewModel model = new ChapterListItemViewModel(this, i)
                    {
                        Name = savedinfo.chapters[i],
                        URL = savedinfo.chapterUrls[i]
                    };

                    model.CheckIfDownloaded();

                    ChapterList.Add(model);
                }
            });
        }

        public async Task LoadMangaInfo(string path)
        {
            mangaSavePath = path;
            if (File.Exists(mangaSavePath))
            {
                savedinfo = SaveSystem.LoadBinary<MgiSave>(mangaSavePath);
                ImportMangaInfo();
            }
        }

        public void SetSelectors(ChapterListItemViewModel item)
        {
            if (_SelectedMemory[0] != null)
            {
                if(_SelectedMemory[1] != null)
                {
                    _SelectedMemory = new ChapterListItemViewModel[2];
                    bool data = !item.IsSelected;
                    for (int i = 0; i < ChapterList.Count; i++)
                    {
                        ChapterList[i].FromMangaView = true;
                        ChapterList[i].IsSelected = false;
                        ChapterList[i].FromMangaView = false;
                    }
                    if(data)
                        return;
                    item.FromMangaView = true;
                    item.IsSelected = true;
                    item.FromMangaView = false;
                    SetSelectors(item);
                }
                else
                {
                    _SelectedMemory[1] = item;
                    int range = _SelectedMemory[0].Index - _SelectedMemory[1].Index;
                    int indexModifier = range > 0 ? -1 : 1;
                    for (int i = 0; i < Math.Abs(range); i++)
                    {
                        int newIndex = _SelectedMemory[0].Index + (i * indexModifier);
                        ChapterList[newIndex].FromMangaView = true;
                        ChapterList[newIndex].IsSelected = true;
                        ChapterList[newIndex].FromMangaView = false;
                    }

                }
            }
            else
            {
                _SelectedMemory[0] = item;
            }
        }

        public void SetImageSource()
        {
            if (_imageArray == null) return;
            ImageSource = _imageArray.ToFreezedBitmapImage();
        }

        public BitmapImage GetImageSource(byte[] array)
        {
            return array.ToFreezedBitmapImage();
        }

        #endregion

        public ObservableCollection<ChapterListItemViewModel> ChapterList
        {
            get => _chapterList;
            set
            {
                if (_chapterList == value) return;
                _chapterList = value;
            }
        }
        public ObservableCollection<GenreItemViewModel> GenreList
        {
            get => _genreList;
            set
            {
                if (_genreList == value) return;
                _genreList = value;
            }
        }
        public ObservableCollection<string> AuthorList
        {
            get => _authorList;
            set
            {
                if (_authorList == value) return;
                _authorList = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                SetMangaPath();
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
            }
        }

        public string MangaStatus
        {
            get { return _mangaStatus; }
            set
            {
                if (_mangaStatus == value) return;
                _mangaStatus = value;
            }
        }

        public string ReadStatus
        {
            get { return _readStatus; }
            set
            {
                if (_readStatus == value) return;
                _readStatus = value;
            }
        }

        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                if (_isRead == value) return;
                _isRead = value;
                ReadStatus = _isRead ? "Continue" : "Read";
            }
        }

        public bool IsFavourite
        {
            get { return _isFavourite; }
            set
            {
                if (_isFavourite == value) return;
                _isFavourite = value;
            }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                if (_isCompleted == value) return;
                _isCompleted = value;
                MangaStatus = _isCompleted ? "Completed" : "Ongoing";
            }
        }

        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (_imageSource == value) return;
                _imageSource = value;
            }
        }
    }
}