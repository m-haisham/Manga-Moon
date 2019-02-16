using HtmlAgilityPack;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Mago
{
    public class ChapterHolder
    {

        public string url;
        public List<string> imagepaths;
        public List<BitmapImage> images;

        public bool pathsLoaded = false;
        public bool imagesLoaded = false;

        public ChapterHolder()
        {
            imagepaths = new List<string>();
            images = new List<BitmapImage>();
        }

        public async Task<bool> DownloadPages()
        {
            WebClient client = new WebClient();
            if (imagepaths .Count == 0) SetImagePaths();

            images = new List<BitmapImage>();
            foreach (var path in imagepaths)
            {
                images.Add(path.ToBitmapImage());
            }
            
            return true;
        }

        private void SetImagePaths()
        {
            List<string> urls = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            HtmlNode idNode = doc.GetElementbyId("vungdoc");
            IEnumerable<HtmlNode> RawPageList =  idNode.Descendants("img");
            for (int i = 0; i < RawPageList.Count(); i++)
            {
                HtmlNode pageNode = RawPageList.ElementAt(i);
                string url = pageNode.Attributes["src"].Value;
                urls.Add(url);
            }

            imagepaths = urls;

        }
    }
}