using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mago
{
    [Serializable]
    public class Settings
    {
        public string mangaPath;
        public bool chapterReaderLoadNotifications;
        public bool chapterDownloadNotifications;
        public bool autoDeleteCompletedDownloads;
        public bool autoDownloadReadChapters;
        public bool darkModeEnabled;

        public Settings()
        {
            mangaPath = "Manga/";
            chapterReaderLoadNotifications = true;
            chapterDownloadNotifications = true;
            autoDeleteCompletedDownloads = true;
            autoDownloadReadChapters = true;
            darkModeEnabled = false;
        }
    }
}
