using System.Threading.Tasks;
using System.Windows.Input;

namespace Mago
{
    public class DownloadedItemViewModel : BaseViewModel
    {
        private string _text;

        private DownloadsViewerViewModel Parent;

        public ICommand Read { get; set; }

        public DownloadedItemViewModel(DownloadsViewerViewModel parent, string text)
        {
            _text = text;
            Parent = parent;
            Read = new RelayCommand(View);
        }

        private void View()
        {
            Task.Run(() => Parent.OpenManga(Text));
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
            }
        }
    }
}