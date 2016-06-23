using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CircuitMVVMBase.MVVM
{
    public abstract class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        static ObservableCollection<Notification> notifications;
        public static ObservableCollection<Notification> Notifications {
            get {
                if (notifications == null)
                {
                    notifications = new ObservableCollection<Notification>();
                }
                return notifications;
            }
        }

        bool isbusy;
        public virtual bool IsBusy {
            get { return isbusy; }
            protected set { RaisePropertyChanged(value, ref isbusy); }
        }

        public string Name { get; set; }

        //private void Notifications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    RaisePropertyChanged("LastNotification");
        //}

        public Notification LastNotification {
            get {
                if (Notifications.Count == 0)
                    return new Notification("...");
                return Notifications[Notifications.Count - 1];
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase()
        {
            Notifications.CollectionChanged += (x, y) => { RaisePropertyChanged("LastNotification"); };
        }


        protected void RaisePropertyChanged(
                              [CallerMemberName]string propertyname = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        protected bool RaisePropertyChanged<T>(T newvalue,
                                            ref T oldvalue,
                                            bool AcceptNull = false,
                                [CallerMemberName]string propertyname = ""
                                )
        {
            if ((newvalue == null && !AcceptNull))
                return false;
            if (newvalue != null && newvalue.Equals(oldvalue))
                return false;
            oldvalue = newvalue;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
            return true;
        }


    }
}
