using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mago
{ 
    public class DownloadsViewerViewModel : BaseViewModel
    {
        private ObservableCollection<DownloadedItemViewModel> _downloadedItems;

        private MainViewModel MainView;

        public DownloadsViewerViewModel(MainViewModel mainView)
        {
            MainView = mainView;
            _downloadedItems = new ObservableCollection<DownloadedItemViewModel>();
            Setup();
        }

        private void Setup()
        {
            if (!Directory.Exists(MainView.Settings.mangaPath)) return;
            string[] items = Directory.GetDirectories(MainView.Settings.mangaPath);
            for (int i = 0; i < items.Length; i++)
            {
                _downloadedItems.Add(new DownloadedItemViewModel(this, items[i].Split('/').Last()));
            }
        }

        public async Task OpenManga(string Header)
        {
            await MainView.MangaViewModel.LoadMangaInfo(MainView.Settings.mangaPath + Header + "/" + Header + ".mgi");
            MainView.MenuViewModel.OpenInfoView();
        }

        public ObservableCollection<DownloadedItemViewModel> DownloadedItems => _downloadedItems;
    }
}
