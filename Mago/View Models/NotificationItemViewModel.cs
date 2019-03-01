using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mago
{
    public class NotificationItemViewModel : BaseViewModel
    {
        private string _message;
        private ColorZoneMode _mode;

        private NotificationsViewModel parent;

        public ICommand Remove { get; set; }

        public NotificationItemViewModel(NotificationsViewModel _parent)
        {
            Remove = new RelayCommand(() => parent.RemoveItem(this));
            parent = _parent;

        }
        
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value) return;
                _message = value;
            }
        }

        public ColorZoneMode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode == value) return;
                _mode = value;
            }
        }
    }
}
