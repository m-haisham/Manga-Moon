using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mago
{
    public class MainViewModel : BaseViewModel
    {
        private DownloadsPanelViewModel _downloadsPanelViewModel;
        private MenuViewModel _menuViewModel;
        private FindByViewModel _findByViewModel;
        private MangaViewModel _mangaViewModel;
        private ReaderViewModel _readerViewModel;
        private NotificationsViewModel _notificationsViewModel;
        private DownloadsViewerViewModel _downloadsViewerViewModel;
        private SettingsPanelViewModel _settingsPanelViewModel;

        public MainViewModel()
        {

            Settings = SaveSystem.LoadSettings();
            _settingsPanelViewModel = new SettingsPanelViewModel(this);

            _downloadsPanelViewModel = new DownloadsPanelViewModel(this);
            _notificationsViewModel = new NotificationsViewModel();
            _menuViewModel = new MenuViewModel(this);
            _findByViewModel = new FindByViewModel(this, HtmlPageLoader);
            _mangaViewModel = new MangaViewModel(this);
            _readerViewModel = new ReaderViewModel(this);
            _downloadsViewerViewModel = new DownloadsViewerViewModel(this);

            HtmlPageLoader = new HtmlPageLoader(this, _mangaViewModel);
            //HtmlPageLoader.LoadData("https://mangakakalot.com/manga/moshi_fanren");
            //HtmlPageLoader.ApplyData();
        }

        public HtmlPageLoader HtmlPageLoader;
        public Settings Settings;

        public DownloadsPanelViewModel DownloadsPanelViewModel => _downloadsPanelViewModel;
        public NotificationsViewModel NotificationsViewModel => _notificationsViewModel;
        public MenuViewModel MenuViewModel => _menuViewModel;
        public FindByViewModel FindByViewModel => _findByViewModel;
        public MangaViewModel MangaViewModel => _mangaViewModel;
        public ReaderViewModel ReaderViewModel => _readerViewModel;
        public DownloadsViewerViewModel DownloadsViewerViewModel => _downloadsViewerViewModel;
        public SettingsPanelViewModel SettingsPanelViewModel => _settingsPanelViewModel;
    }
}
