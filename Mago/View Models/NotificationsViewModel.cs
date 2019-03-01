using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mago
{
    public class NotificationsViewModel : BaseViewModel
    {
        private ObservableCollection<NotificationItemViewModel> _notifications;

        private Task _autoRemove;
        private float delayTime = 5f;

        public NotificationsViewModel()
        {
            _notifications = new ObservableCollection<NotificationItemViewModel>();
        }

        public void Setup()
        {
            for (int i = 0; i < 5; i++)
            {
                AddNotification("Notification number: " + (i + 1).ToString(), NotificationMode.Normal);
            }
        }

        public void AddNotification(string message, NotificationMode mode)
        {
            _notifications.Add(new NotificationItemViewModel(this) { Message = message });
            switch (mode)
            {
                case NotificationMode.Normal:
                    _notifications.Last().Mode = MaterialDesignThemes.Wpf.ColorZoneMode.PrimaryMid;
                    break;

                case NotificationMode.Success:
                    _notifications.Last().Mode = MaterialDesignThemes.Wpf.ColorZoneMode.Accent;
                    break;

                case NotificationMode.Error:
                    _notifications.Last().Mode = MaterialDesignThemes.Wpf.ColorZoneMode.Inverted;
                    break;
            }
            if(_autoRemove == null)
            {
                _autoRemove = Task.Run(AutoRemove);
            }
        }

        async Task AutoRemove()
        {
            while(_notifications.Count > 0)
            {
                //remember count before waiting
                int lastCount = _notifications.Count;

                //wait for x seconds
                await Task.Delay((int)(delayTime * 1000));

                //if item count not same as before, go back to start
                if (_notifications.Count != lastCount) continue;

                //call main thread to remove item
                Application.Current.Dispatcher.Invoke(() => _notifications.RemoveAt(0));
            }

            _autoRemove = null;

        } 

        public void RemoveItem(NotificationItemViewModel notification)
        {
            _notifications.Remove(notification);
        }

        public ObservableCollection<NotificationItemViewModel> Notifications => _notifications;

    }
}
