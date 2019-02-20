using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mago
{
    public class ChapterListItemViewModel : BaseViewModel
    {
        public int Index;
        public bool FromMangaView = false;

        private string _name;

        private bool _isRead;
        private bool _isSelected;
        private bool _isNotDownloaded;

        private string _url;
        private MangaViewModel Parent;

        public ICommand DownloadChapter { get; set; }
        public ICommand ReadChapter { get; set; }

        public ChapterListItemViewModel(MangaViewModel parent, int index)
        {
            Parent = parent;
            Index = index;

            DownloadChapter = new RelayCommand(AddToDownloadQueue);
            ReadChapter = new RelayCommand(Read);
        }

        private void AddToDownloadQueue()
        {
            Parent.AddtoDownloads(this);
            IsNotDownloaded = false;
        }

        private void Read()
        {
            Task.Run(() => Parent.OpenInReader(Index));
            IsRead = true;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
            }
        }
        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                if (_isRead == value) return;
                _isRead = value;
            }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                if (!FromMangaView)
                    Parent.SetSelectors(this);
            }
        }
        public bool IsNotDownloaded
        {
            get { return _isNotDownloaded; }
            set
            {
                if (_isNotDownloaded == value) return;
                _isNotDownloaded = value;
            }
        }
        public string URL
        {
            get { return _url; }
            set
            {
                if (_url == value) return;
                _url = value;
            }
        }
        
    }
}
