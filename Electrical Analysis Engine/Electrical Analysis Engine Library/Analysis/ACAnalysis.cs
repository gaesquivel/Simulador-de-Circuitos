using System;
using ElectricalAnalysis.Analysis.Solver;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;
using ElectricalAnalysis.Components;

namespace ElectricalAnalysis
{
    public class ACAnalysis:BasicAnalysis
    {

        public enum ACAnalysisScan { Decade, Linear }

        protected ACAnalysisScan scantype;

        public string StartFrequency { get; set; }
        public string EndFrequency { get; set; }
        public int Points{ get; set; }
        public ACAnalysisScan ScanType { 
            get { return scantype; }
            set {
                if (value == ACAnalysisScan.Linear)
                    StartFrequency = "0";
                else
                    StartFrequency = "10";
            } 
        }

        protected override string DefaultName { get { return "AC Analysis"; } }

        public ACAnalysis():base()
        {
            //Name = "AC Analysis " + ID.ToString();
            StartFrequency = "10";
            EndFrequency = "10Meg";
            Points = 101;
            ScanType = ACAnalysisScan.Decade;
            Solver= new ACSweepSolver();
        }


        public override object Clone()
        {
            ACAnalysis clon = new ACAnalysis();
            clon.EndFrequency = EndFrequency;
            clon.ScanType = ScanType;
            clon.StartFrequency = StartFrequency;
            //clon. = EndFrequency;
            clon.Points = Points;
            return clon;
        }

        public override bool Parse(Circuit owner, string details)
        {
            //.AC dec points startf stopf
            string[] anali = details.ToLower().Substring(2).Split(" ".ToCharArray(), 
                                            StringSplitOptions.RemoveEmptyEntries);

            if (anali[0] != "ac")
            {
                NotificationsVM.Instance.Notifications.Add(
                   new Notification("Invalid analisys type", Notification.ErrorType.warning));
                return false;
            }
            ACAnalysis setupac = null;

            foreach (var itm in owner.Setup)
            {
                if (itm is ACAnalysis)
                {
                    setupac = itm as ACAnalysis;
                    break;
                }
            }
            if (setupac == null)
            {
                setupac = new ACAnalysis();
                owner.Setup.Add(setupac);
            }

            string result = "";
            double val = 0;

            switch (anali[1])
            {
                case "linear":
                    setupac.ScanType = ACAnalysisScan.Linear;
                    break;
                case "dec":
                default:
                    setupac.ScanType = ACAnalysisScan.Decade;
                    break;
            }

            if (!ParseUtils.ParseValue(anali, 2, out val, 101, true))
                return false;
            setupac.Points = (int)val;

            //if (!ParseUtils.ParseValue(anali, 2, out val, 101, true))
            //    return false;
            if (!ParseUtils.ParseStringValue(anali, 3, ref result, "1", false))
                return false;
            setupac.StartFrequency = result;

            if (!ParseUtils.ParseStringValue(anali, 4, ref result, "1meg", false))
                return false;
            setupac.EndFrequency = result;
            


            return true;
        }

    }
}
