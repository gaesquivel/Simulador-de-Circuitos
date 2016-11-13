using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System.Numerics;

namespace ElectricalAnalysis.Components.Generators
{
    public abstract class TransientGenerator: ACVoltageGenerator
    {
        protected double amplitude, f, offset;


        public string Delay { get; set; }
        public string Amplitude { get; set; }
        public string Offset { get; set; }
        public string Frequency { get; set; }
        public string Cicles { get; set; }
        protected bool wasparsed;

        public TransientGenerator(ComponentContainer owner, string name)
            : base(owner)
        {
            Initialize(name);
            Amplitude = "10";
            Frequency = "1K";
            Delay = "10u";
            Offset = "0";
            Cicles = "0";
            Value = 0;
        }


        public override Complex Voltage
        {
            get
            {
                if (!wasparsed)
                {
                    Parse();
                }
                return base.Voltage;
            }
        }


        protected virtual void Parse()
        {
            if (!StringUtils.DecodeString(Amplitude, out amplitude))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Amplitude: " + Amplitude));
                return;
            }
            if (!StringUtils.DecodeString(Frequency, out f))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Frequency: " + Frequency));
                return;
            }
           
            if (!StringUtils.DecodeString(Offset, out offset))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Offset: " + Offset));
                return;
            }
            wasparsed = true;
        }

        public override void Reset()
        {
            wasparsed = false;
            base.Reset();
        }

    }
}
