using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ComplexPlainVisualizer
{
    public abstract class ViewModelBase: INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(
                              [CallerMemberName]string propertyname = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        protected void RaisePropertyChanged<T>(T newvalue,
                                            ref T oldvalue, 
                                [CallerMemberName]string propertyname = "")
        {
            if (newvalue.Equals(oldvalue))
                return;
            oldvalue = newvalue;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        
    }
}
