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
        public static BitmapImage ToBitmapImage(this byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public static BitmapImage ToBitmapImage(this string url)
        {
            WebClient web = new WebClient();
            byte[] array = web.DownloadData(url);

            using (MemoryStream ms = new MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
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
