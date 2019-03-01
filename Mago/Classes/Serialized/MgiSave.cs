using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Mago
{
    [Serializable]
    public class MgiSave
    {
        public string url;
        public string name;
        public string description;
        public byte[] image;
        public bool isFavourite;
        public bool isCompleted;
        public ObservableCollection<string> authors;
        public ObservableCollection<string> genres;
        public ObservableCollection<string> chapters;
        public ObservableCollection<string> chapterUrls;

        public MgiSave()
        {
            authors = new ObservableCollection<string>();
            genres = new ObservableCollection<string>();
            chapters = new ObservableCollection<string>();
            chapterUrls = new ObservableCollection<string>();
        }

        public MgiSave(string url, string name, string description, bool isFavourite, bool isCompleted)
        {
            this.url = url;
            this.name = name;
            this.description = description;
            this.isFavourite = isFavourite;
            this.isCompleted = isCompleted;
            authors = new ObservableCollection<string>();
            genres = new ObservableCollection<string>();
            chapters = new ObservableCollection<string>();
            chapterUrls = new ObservableCollection<string>();
        }
    }
}
