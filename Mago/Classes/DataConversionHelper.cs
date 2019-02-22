using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Mago
{
    public static class DataConversionHelper
    {
        static MemoryStream ms;
        static BitmapImage image;

        public static BitmapImage ToFreezedBitmapImage(this byte[] array)
        {
            image = new BitmapImage();
            using (ms = new MemoryStream(array))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
            }
            return image;
        }

        public static BitmapImage ToBitmapImage(this string url)
        {
            WebClient web = new WebClient();
            byte[] array = web.DownloadData(url);

            image = new BitmapImage();
            using (ms = new MemoryStream(array))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public static byte[] DownloadToArray(string url)
        {
            WebClient web = new WebClient();
            return web.DownloadData(url);
        }

        public static BitmapImage ToFreezedBitmapImage(this string url)
        {
            var image = url.ToBitmapImage();
            image.Freeze();
            return image;
        }

        public static async Task<List<ChapterHolder>> ToRawChapters(this string mangaURL)
        {
            List<ChapterHolder> chapterHolders = new List<ChapterHolder>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(mangaURL);

            IEnumerable<HtmlNode> chapterCollection = doc.DocumentNode.SelectNodes("//div[@class='chapter-list']").Descendants("div");

            ChapterHolder newChapter;
            foreach (var node in chapterCollection)
            {
                newChapter = new ChapterHolder();
                HtmlNode link = node.Descendants("span").First().Descendants("a").First();
                newChapter.url = link.Attributes["href"].Value;

                chapterHolders.Add(newChapter);
            }

            return chapterHolders;

        }
    }
}
