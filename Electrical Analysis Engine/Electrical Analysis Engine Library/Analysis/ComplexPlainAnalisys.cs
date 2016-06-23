using ElectricalAnalysis.Analysis.Solver;
using System.ComponentModel;

namespace ElectricalAnalysis.Analysis
{
    public class ComplexPlainAnalysis:BasicAnalysis
    {
        string wmax, wmin, smax, smin;
        public string WMax {
            get { return wmax; }
            set {
                RaisePropertyChanged(value, ref wmax);
            }
        }
        public string WMin
        {
            get { return wmin; }
            set
            {
                RaisePropertyChanged(value, ref wmin);
            }
        }
        public string SigmaMax
        {
            get { return smax; }
            set
            {
                RaisePropertyChanged(value, ref smax);
            }
        }
        public string SigmaMin
        {
            get { return smin; }
            set
            {
                RaisePropertyChanged(value, ref smin);
            }
        }

        [Description("Get or Set all plain limits at same time")]
        public string MaxValue
        {
            get {
                //if (SigmaMax == -SigmaMin && WMax == -WMin &&
                //    WMax == SigmaMax)
                    return SigmaMax;
                //return StringUtils.CodeString(SigmaMax) + ", " +
                //    StringUtils.CodeString(SigmaMin) + "; " +
                //    StringUtils.CodeString(WMin) + ", " +
                //    StringUtils.CodeString(WMin);
            }
            set
            {
                double val = 0;
                if (StringUtils.DecodeString(value, out val))
                {
                    SigmaMax = WMax = value;
                    SigmaMin = WMin = "-" + value;
                    //RaisePropertyChanged(val ,ref SigmaMax, "SigmaMax");
                    //RaisePropertyChanged("SigmaMax");
                    //RaisePropertyChanged("SigmaMin");
                    //RaisePropertyChanged("WMax");
                    //RaisePropertyChanged("WMin");
                    RaisePropertyChanged();
                }
            }
        } 
        protected override string DefaultName { get { return "AC Plain Analysis"; } }

        public int Points { get; set; }

        public ComplexPlainAnalysis()
            : base()
        {
            WMax = SigmaMax = "100K";
            WMin = SigmaMin = "-100K";
            Points = 91;
            Solver = new ComplexPlainSolver();
        }

        public override object Clone()
        {
            ComplexPlainAnalysis clon = new ComplexPlainAnalysis();
            clon.WMax = WMax;
            clon.WMin = WMin;
            clon.SigmaMax = SigmaMax;
            clon.SigmaMin = SigmaMin;
            clon.Points = Points;
            return clon;
        }
    }
}
