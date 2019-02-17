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

        public MainViewModel()
        {
            _downloadsPanelViewModel = new DownloadsPanelViewModel();
            _menuViewModel = new MenuViewModel(this);
            _findByViewModel = new FindByViewModel(this, HtmlPageLoader);
            _mangaViewModel = new MangaViewModel(this);
            _readerViewModel = new ReaderViewModel();
            HtmlPageLoader = new HtmlPageLoader(_mangaViewModel);
            
            //HtmlPageLoader.LoadData("https://mangakakalot.com/manga/moshi_fanren");
            //HtmlPageLoader.ApplyData();
        }

        public HtmlPageLoader HtmlPageLoader;

        public DownloadsPanelViewModel DownloadsPanelViewModel => _downloadsPanelViewModel;
        public MenuViewModel MenuViewModel => _menuViewModel;
        public FindByViewModel FindByViewModel => _findByViewModel;
        public MangaViewModel MangaViewModel => _mangaViewModel;
        public ReaderViewModel ReaderViewModel => _readerViewModel;
        
    }
}
