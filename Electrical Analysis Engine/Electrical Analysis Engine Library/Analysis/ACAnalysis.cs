using System;
using ElectricalAnalysis.Analysis.Solver;

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
    }
}
