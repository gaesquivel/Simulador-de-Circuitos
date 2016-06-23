using System;
using System.Numerics;

namespace ElectricalAnalysis.Components.Generators
{

    public class SineVoltageGenerator : ACVoltageGenerator
    {
        double amplitude, w, thau, offset, phase;
        bool wasparsed;


        public string Delay { get; set; }
        public string Amplitude { get; set; }
        public string Offset { get; set; }
        public string Frequency { get; set; }
        public string Phase { get; set; }
        public string Thau { get; set; }

        public SineVoltageGenerator(ComponentContainer owner, string name)
            : base(owner)
        {
            Initialize(name);
            Amplitude = "10";
            Frequency = "1K";
            Phase = "0";
            Thau = "0";
            Delay = "10u";
            Offset = "0";
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

        protected virtual void Parse()
        {
            amplitude = StringUtils.DecodeString(Amplitude);
            w = 2 * Math.PI * StringUtils.DecodeString(Frequency);
            thau = StringUtils.DecodeString(Thau);
            wasparsed = true;
            offset = StringUtils.DecodeString(Offset);
            phase = StringUtils.DecodeString(Phase);
        }

        public override void Reset()
        {
            wasparsed = false;
            base.Reset();
        }
    }
}
