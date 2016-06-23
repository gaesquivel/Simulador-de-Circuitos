//using System.Numerics;

using CircuitMVVMBase;
using System.Windows;

namespace ElectricalAnalysis.Components
{
    public abstract class ElectricComponent: Dipole
    {
        public static DependencyProperty valorDP = DependencyProperty.Register("Value",
        typeof(double), typeof(ElectricComponent));

        public double Value
        {
            get { return (double)GetValue(valorDP); }
            set { SetValue(valorDP, value); }
        }


        //Expresion R, L, C
        public virtual string Expresion { get; set; }   //model?
       // public double Value { get; set; }   //1000 (ohms, Henry...)
        public double Temperature { get; set; }




        public ElectricComponent(ComponentContainer owner)
            : base(owner)
        { 
        }

        protected void Initialize(string name = null, string value = null)
        { 
            if (string.IsNullOrEmpty(name))
                Name = "Component" + ID.ToString();
            else
                Name = name;

            if (string.IsNullOrEmpty(value))
                Value = 0;
            else
            {
                double val = 0;
                if (StringUtils.DecodeString(value, out val))
                    Value = val;
                else
                    Notifications.Add(new Notification("Invalid value :" + 
                        value, Notification.ErrorType.warning));
            }
        }

    }
}
