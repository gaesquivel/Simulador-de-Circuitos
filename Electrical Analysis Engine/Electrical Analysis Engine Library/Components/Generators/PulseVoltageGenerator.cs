using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System;
using System.Numerics;

namespace ElectricalAnalysis.Components.Generators
{

    public class PulseVoltageGenerator : TransientGenerator
    {
        double period = 0, rise, fall, Ton;
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
            //OnTime = "0";
            Value = 0;
        }


        public override double voltage(NodeSingle referenceNode, double t)
        {
            if (!wasparsed )
            {
                Parse();
            }


            double v = offset;
            if (t % (period) > (Ton))
                v += amplitude;

            if (referenceNode ==  Nodes[0])
                return v;
            else
                return -v;
        }

        protected override void Parse()
        {
            base.Parse();
            //if (!StringUtils.DecodeString(OnTime, out Ton))
            //{
            //    NotificationsVM.Instance.Notifications.Add(
            //        new Notification("Error to parse T On: " + OnTime));
            //    return;
            //}
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
            if (f > 0)
                period = 1 / f;
            //if (Ton == 0)
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
