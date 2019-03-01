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
        //theme
        public bool darkModeEnabled;
        
        //reader
        public int ReaderZoomPercent;
        public bool autoDownloadReadChapters;
        public bool autoDownloadNextChapter;

        //downloader
        public string mangaPath;
        public bool autoDeleteCompletedDownloads;

        //notifications
        public bool chapterReaderLoadNotifications;

        public bool chapterDownloadNotifications;
        public bool downloadTaskNotifications;

        public Settings()
        {
            Reset();
        }

        public void Reset()
        {
            mangaPath = "Manga/";
            ReaderZoomPercent = 100;
            chapterReaderLoadNotifications = true;
            chapterDownloadNotifications = true;
            autoDeleteCompletedDownloads = true;
            autoDownloadReadChapters = true;
            autoDownloadNextChapter = true;
            darkModeEnabled = false;
        }
    }
}
