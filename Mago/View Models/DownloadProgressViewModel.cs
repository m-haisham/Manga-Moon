using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mago
{
    public class DownloadProgressViewModel : BaseViewModel
    {
        private DownloadState _DownloadState;
        private bool _isSelected;
        private string _header;
        private string _description;
        private string _code;
        private int _progress;
        private int _maximum;

        public bool IsIndeterminate { get { return _DownloadState == DownloadState.Queued ? true : false; } }

        public DownloadState DownloadState
        {
            get { return _DownloadState; }
            set
            {
                if (_DownloadState == value) return;
                _DownloadState = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
            }
        }

        public string Header
        {
            get { return _header; }
            set
            {
                if (_header == value) return;
                _header = value;
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
            }
        }
        

        public string Code
        {
            get { return _code; }
            set
            {
                if (_code == value) return;
                _code = value;
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                if (_progress == value) return;
                _progress = value;
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum == value) return;
                _maximum = value;
            }
        }

    }
}
