using ElectricalAnalysis.Analysis.Solver;
using System.ComponentModel;
using ElectricalAnalysis.Components;
using System;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;

namespace ElectricalAnalysis.Analysis
{
    public class ComplexPlainAnalysis:BasicAnalysis
    {
        string wmax, wmin, smax, smin;

        #region properties

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
                    return SigmaMax;
            }
            set
            {
                double val = 0;
                if (StringUtils.DecodeString(value, out val))
                {
                    SigmaMax = WMax = value;
                    SigmaMin = WMin = "-" + value;
                    RaisePropertyChanged();
                }
            }
        } 
        protected override string DefaultName { get { return "AC Plain Analysis"; } }

        public int Points { get; set; }

        #endregion

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

        public override bool Parse(Circuit owner, string details)
        {
            //.ACPlain [points] [stopW] [stopSigma] [startW] [startSigma]
            string[] anali = details.ToLower().Substring(2).Split(" ".ToCharArray(),
                                            StringSplitOptions.RemoveEmptyEntries);

            if (anali[0] != "acplain")
            {
                NotificationsVM.Instance.Notifications.Add(
                   new Notification("Invalid analisys type", Notification.ErrorType.warning));
                return false;
            }
            ComplexPlainAnalysis setupac = null;

            foreach (var itm in owner.Setup)
            {
                if (itm is ComplexPlainAnalysis)
                {
                    setupac = itm as ComplexPlainAnalysis;
                    break;
                }
            }
            if (setupac == null)
            {
                setupac = new ComplexPlainAnalysis();
                owner.Setup.Add(setupac);
            }

            string result = "";
            double val = 0;

           

            if (!ParseUtils.ParseValue(anali, 1, out val, 91, true))
                return false;
            setupac.Points = (int)val;

           
            if (!ParseUtils.ParseStringValue(anali, 2, ref result, "100K", false))
                return false;
            setupac.WMax = result;

            if (!ParseUtils.ParseStringValue(anali, 3, ref result, result, false))
                return false;
            setupac.SigmaMax = result;

            if (!ParseUtils.ParseStringValue(anali, 4, ref result, "-" + result, false))
                return false;
            setupac.WMin = result;

            if (!ParseUtils.ParseStringValue(anali, 5, ref result, result, false))
                return false;
            setupac.SigmaMin = result;


            return true;
        }
    }
}
