using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;

namespace Mago
{
    public class HtmlPageLoader
    {
        private string url;
        private MangaViewModel MangaView;
        private MainViewModel MainView;

        private string _name;
        private string _status;
        private BitmapImage _image;
        private string _description;

        private ObservableCollection<ChapterListItemViewModel> _chapterList;
        private ObservableCollection<GenreItemViewModel> _genreList;
        private ObservableCollection<string> _authorList;

        public HtmlPageLoader(MainViewModel mainView, MangaViewModel mangaView)
        {
            MainView = mainView;
            MangaView = mangaView;
        }

        public void Feed(string _url)
        {
            url = _url;
        }

        public async Task LoadData()
        {

            #region Get Data
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            IEnumerable<HtmlNode> infonodes = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'manga-info-text')]").Descendants("li");

            //Get Name
            string name = infonodes.ElementAt(0).Descendants("h1").FirstOrDefault().InnerText;

            //get Image URL
            HtmlNode img = doc.DocumentNode.SelectSingleNode("//div[@class='manga-info-pic']").Descendants("img").First();
            string imageSource = img.Attributes["src"].Value;

            //Get Status
            string status = infonodes.ElementAt(2).InnerText.Substring(9);

            //Get Authors
            ObservableCollection<string> authors = new ObservableCollection<string>();

            IEnumerable<HtmlNode> authorNodes = infonodes.ElementAt(1).Descendants("a");
            for (int i = 0; i < authorNodes.Count(); i++)
            {
                authors.Add(authorNodes.ElementAt(i).InnerText);
            }

            //Get Genres
            ObservableCollection<string> genres = new ObservableCollection<string>();

            IEnumerable<HtmlNode> genreNodes = infonodes.ElementAt(6).Descendants("a");
            for (int i = 0; i < genreNodes.Count(); i++)
            {
                genres.Add(genreNodes.ElementAt(i).InnerText);
            }

            //Get Description
            HtmlNodeCollection descriptionNodes = doc.DocumentNode.SelectNodes("//div[@id='noidungm']/text()");

            StringBuilder sb = new StringBuilder();

            foreach (var node in descriptionNodes)
            {
                sb.Append(node.InnerText);
            }

            string description = Regex.Replace(sb.ToString(), @"\n|&#39;|&quot;", "");
            description = Regex.Replace(description, @"&#39;|&rsquo;", "'");

            //Get Chapter List
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
            chapters.Reverse();
            #endregion

            #region Save Data
            _name = name;
            _status = status;
            _image = imageSource.ToFreezedBitmapImage();
            _description = description;

            _authorList = authors;

            _genreList = new ObservableCollection<GenreItemViewModel>();
            foreach (var genre in genres)
            {
                _genreList.Add(new GenreItemViewModel
                {
                    Text = genre
                });
            }

            _chapterList = new ObservableCollection<ChapterListItemViewModel>();
            for (int i = 0; i < chapters.Count; i++)
            {
                _chapterList.Add(
                    new ChapterListItemViewModel(MangaView, i)
                    {
                        Name = chapters[i].Name,
                        URL = chapters[i].href,
                        IsNotDownloaded = true
                    }
                );
            }
            #endregion
        }

        public async Task LoadData(string _url)
        {

            url = _url;

            #region Get Data
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(_url);

            IEnumerable<HtmlNode> infonodes = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'manga-info-text')]").Descendants("li");

            //Get Name
            string name = infonodes.ElementAt(0).Descendants("h1").FirstOrDefault().InnerText;

            //get Image URL
            HtmlNode img = doc.DocumentNode.SelectSingleNode("//div[@class='manga-info-pic']").Descendants("img").First();
            string imageSource = img.Attributes["src"].Value;

            //Get Status
            string status = infonodes.ElementAt(2).InnerText.Substring(9);

            //Get Authors
            ObservableCollection<string> authors = new ObservableCollection<string>();

            IEnumerable<HtmlNode> authorNodes = infonodes.ElementAt(1).Descendants("a");
            for (int i = 0; i < authorNodes.Count(); i++)
            {
                authors.Add(authorNodes.ElementAt(i).InnerText);
            }

            //Get Genres
            ObservableCollection<string> genres = new ObservableCollection<string>();

            IEnumerable<HtmlNode> genreNodes = infonodes.ElementAt(6).Descendants("a");
            for (int i = 0; i < genreNodes.Count(); i++)
            {
                genres.Add(genreNodes.ElementAt(i).InnerText);
            }

            //Get Description
            HtmlNodeCollection descriptionNodes = doc.DocumentNode.SelectNodes("//div[@id='noidungm']/text()");

            StringBuilder sb = new StringBuilder();

            foreach (var node in descriptionNodes)
            {
                sb.Append(node.InnerText);
            }

            string description = Regex.Replace(sb.ToString(), @"\n|&#39;|&quot;", "");
            description = Regex.Replace(description, @"&#39;|&rsquo;", "'");

            //Get Chapter List
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
            chapters.Reverse();
            #endregion

            #region Save Data
            _name = name;
            _status = status;
            _image = imageSource.ToFreezedBitmapImage();
            _description = description;

            _authorList = authors;

            _genreList = new ObservableCollection<GenreItemViewModel>();
            foreach (var genre in genres)
            {
                _genreList.Add(new GenreItemViewModel
                {
                    Text = genre
                });
            }

            _chapterList = new ObservableCollection<ChapterListItemViewModel>();
            for (int i = 0; i < chapters.Count; i++)
            {
                ChapterListItemViewModel model = new ChapterListItemViewModel(MangaView, i)
                {
                    Name = chapters[i].Name,
                    URL = chapters[i].href
                };

                model.CheckIfDownloaded();

                _chapterList.Add(model);
            }
            #endregion
        }

        public async Task ApplyData()
        {
            MangaView.Url = url;

            MangaView.Name = _name;
            MangaView.MangaStatus = _status;
            MangaView.ImageSource = _image;
            MangaView.Description = _description;

            Application.Current.Dispatcher.Invoke(() =>
            {
                MangaView.ChapterList = _chapterList;

                MangaView.GenreList = _genreList;
                MangaView.AuthorList = _authorList;
            });
        }
    }
}