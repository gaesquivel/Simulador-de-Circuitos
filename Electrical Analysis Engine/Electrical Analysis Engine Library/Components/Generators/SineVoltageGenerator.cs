using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System;
using System.Numerics;

namespace ElectricalAnalysis.Components.Generators
{

    public class SineVoltageGenerator : TransientGenerator
    {
        double  w, thau, phase;
      
        public string Phase { get; set; }
        public string Thau { get; set; }

        public SineVoltageGenerator(ComponentContainer owner, string name)
            : base(owner, name)
        {
            Initialize(name);
           
            Phase = "0";
            Thau = "0";
            
            Value = 0;
        }


        public override double voltage(NodeSingle referenceNode, double t)
        {
            if (!wasparsed )
            {
                Parse();
            }

            double v = offset + amplitude * Math.Sin(w * t + phase / (2 * Math.PI));
            if (referenceNode ==  Nodes[0])
                return v;
            else
                return -v;
        }

        protected override void Parse()
        {
            base.Parse();
            if (!StringUtils.DecodeString(Thau, out thau))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Thau: " + Thau));
                return;
            }
           
            if (!StringUtils.DecodeString(Phase, out phase))
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Error to parse Phase: " + Phase));
                return;
            }
            w = f * 2 * Math.PI;
            wasparsed = true;
        }

        public override void Reset()
        {
            wasparsed = false;
            base.Reset();
        }
    }
}
