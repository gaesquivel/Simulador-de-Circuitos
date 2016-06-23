using CircuitMVVMBase.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Laplace
{
    public class Singularity:ViewModelBase
    {
        public enum SingularityTypes { Pole, Zero }


        Complex _value;
        public Complex Value
        {
            get { return _value; }
            set { RaisePropertyChanged(value, ref _value); }
        }

        int order;
        public int Order
        {
            get { return order; }
            set {
                if (value <= 0)
                    return;
                RaisePropertyChanged(value, ref order);
            }
        }

        SingularityTypes singularitytype;
        public SingularityTypes SingularityType
        {
            get { return singularitytype; }
            set { RaisePropertyChanged(value, ref singularitytype); }
        }

        public Singularity(Complex ?Value = null ,
                            int order = 1, 
                            SingularityTypes type = SingularityTypes.Pole)
        {
            SingularityType = type;
            Order = order;
            if (Value != null)
                this.Value = (Complex)Value;
        }

    }
}
