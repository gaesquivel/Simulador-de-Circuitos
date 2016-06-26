using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitMVVMBase.MVVM
{
    public class NotificationsVM: ViewModelBase
    {
        static NotificationsVM instance;
        public static NotificationsVM Instance
        {
            get
            {
                if (instance == null)
                    instance = new NotificationsVM();
                return instance;
            }
        }

        ObservableCollection<Notification> notifications;
        public ObservableCollection<Notification> Notifications
        {
            get
            {
                if (notifications == null)
                {
                    notifications = new ObservableCollection<Notification>();
                    notifications.CollectionChanged += Notifications_CollectionChanged;
                }
                return notifications;
            }
        }

        private void Notifications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("LastNotification");
        }

        public Notification LastNotification
        {
            get
            {
                if (Notifications.Count == 0)
                    return new Notification("...");
                return Notifications[Notifications.Count - 1];
            }
        }

    }
}
