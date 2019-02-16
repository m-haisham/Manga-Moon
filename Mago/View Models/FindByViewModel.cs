using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HtmlAgilityPack;

namespace Mago
{
    public class FindByViewModel : BaseViewModel
    {
        private string _mangaName;
        private string _mangaURL;
        private ObservableCollection<string> _suitableMangaSources;
        private int _selectedIndex = 0;
        private Visibility _warningIconVisibility = Visibility.Hidden;
        private bool _URLIsIndeternimate;
        private bool _URLButtonEnabled = true;
        private bool _NameIsIndeternimate;
        private bool _NameButtonEnabled = true;

        private MainViewModel main;
        private HtmlPageLoader PageLoader;

        public ICommand FindByName { get; set; }
        public ICommand FindByURL { get; set; }

        public FindByViewModel(MainViewModel mainViewModel, HtmlPageLoader pageLoader)
        {
            _suitableMangaSources = new ObservableCollection<string> { "Mangakakalot.com" };

            FindByName = new RelayCommand(() => Task.Run(SearchWithName));
            FindByURL = new RelayCommand(() => Task.Run(SearchWithURL));

            main = mainViewModel;
        }

        async Task SearchWithName()
        {
            if (MangaName == null)
                return;
            NameIsIndeterminate = true;
            string url = ComposeURL(MangaName);
            bool isWebsiteValid = await RemoteFileExists(url);
            if (!isWebsiteValid) { NameIsIndeterminate = false; WarningIconVisibility = Visibility.Visible; return; }
            WarningIconVisibility = Visibility.Hidden;
            NameIsIndeterminate = false;
            OpenManga(url);
        }

        async Task SearchWithURL()
        {
            if (MangaURL == null)
                return;
            URLIsIndeterminate = true;
            bool isWebsiteValid = await RemoteFileExists(MangaURL);
            if (!isWebsiteValid) { URLIsIndeterminate = false; WarningIconVisibility = Visibility.Visible; return; }
            WarningIconVisibility = Visibility.Hidden;
            URLIsIndeterminate = false;
            OpenManga(MangaURL);
        }

        private void OpenManga(string url)
        {
            Debug.WriteLine(url);
        }

        async Task<bool> RemoteFileExists(string url)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                HtmlNodeCollection attributes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'chapter-list')]");
                if (attributes != null)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        private string ComposeURL(string name)
        {
            name = name.Replace(' ', '_').ToLower();
            return "https://" + SuitableMangaSources[SelectedIndex].ToLower() + (SuitableMangaSources[SelectedIndex] == "Mangakakalot.com" ? "/manga" : "") + "/" + name;
        }

        public ObservableCollection<string> SuitableMangaSources => _suitableMangaSources;

        public string MangaName
        {
            get { return _mangaName; }
            set
            {
                if (_mangaName == value) return;
                _mangaName = value;
            }
        }
        public string MangaURL
        {
            get { return _mangaURL; }
            set
            {
                if (_mangaURL == value) return;
                _mangaURL = value;
            }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
            }
        }
        public Visibility WarningIconVisibility
        {
            get { return _warningIconVisibility; }
            set
            {
                if (_warningIconVisibility == value) return;
                _warningIconVisibility = value;
            }
        }
        public bool URLIsIndeterminate
        {
            get { return _URLIsIndeternimate; }
            set
            {
                if (_URLIsIndeternimate == value) return;
                _URLIsIndeternimate = value;
                URLButtonEnabled = !_URLIsIndeternimate;
            }
        }
        public bool NameIsIndeterminate
        {
            get { return _NameIsIndeternimate; }
            set
            {
                if (_NameIsIndeternimate == value) return;
                _NameIsIndeternimate = value;
                NameButtonEnabled = !_NameIsIndeternimate;

            }
        }
        public bool URLButtonEnabled
        {
            get { return _URLButtonEnabled; }
            set
            {
                if (_URLButtonEnabled == value) return;
                _URLButtonEnabled = value;
            }
        }
        public bool NameButtonEnabled
        {
            get { return _NameButtonEnabled; }
            set
            {
                if (_NameButtonEnabled == value) return;
                _NameButtonEnabled = value;
            }
        }
    }
}
