using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System;
using System.Numerics;

namespace ElectricalAnalysis.Components.Generators
{

    public class PulseVoltageGenerator : TransientGenerator, IPulseGenerator
    {
        double period = 0, rise, fall, Ton;
        double cicles = 0;
        public string RiseTime { get; set; }
        public string FallTime { get; set; }

        float _ton = 50;
        public float OnTime {
            get { return _ton; }
            set {
                if (value >= 0 && value <= 100)
                    _ton = value;
                else
                    NotificationsVM.Instance.Notifications.Add(
                    new Notification("TOn must be betwen 0 and 100%: " + value.ToString()));
            }
        }

        public PulseVoltageGenerator(ComponentContainer owner, string name)
            : base(owner, name)
        {
            Initialize(name);

            RiseTime = "0";
            FallTime = "0";
            Value = 0;
        }


        public override double voltage(NodeSingle referenceNode, double t)
        {
            if (!wasparsed )
            {
                Parse();
            }

            double v = offset;
            if (cicles > 0 && (t / period > cicles))
            { }
            else
            {
                double deltat = t % (period);
                if (deltat < (rise))
                    v += amplitude * deltat / rise;
                else if (deltat < (Ton))
                    v += amplitude;
                else if (deltat < (rise + Ton + fall))
                    v += amplitude - amplitude * (deltat - rise - Ton) / fall;
            }
            
            if (referenceNode ==  Nodes[0])
                return v;
            else
                return -v;
        }

        protected override void Parse()
        {
            base.Parse();
           
            if (!StringUtils.DecodeString(RiseTime, out rise))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Rise Time: " + RiseTime));
                return;
            }
            if (!StringUtils.DecodeString(FallTime, out fall))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Fall Time: " + FallTime));
                return;
            }
            if (!StringUtils.DecodeString(Cicles, out cicles))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Numbre of Cicles: " + Cicles));
                return;
            }
            //cicles = 
            if (f > 0)
                period = 1 / f;
            Ton = period * _ton / 100;

            wasparsed = true;
        }

        public override void Reset()
        {
            wasparsed = false;
            base.Reset();
        }
    }
}
