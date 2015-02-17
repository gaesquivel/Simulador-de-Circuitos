using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;

namespace ElectricalAnalysis
{
    public class ACAnalysis:BasicAnalysis
    {
        protected ACAnalysisScan scantype;

        public enum ACAnalysisScan { Decade, Linear }


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

        public ACAnalysis():base()
        {
            Name = "AC Analysis " + ID.ToString();
            StartFrequency = "10";
            EndFrequency = "10Meg";
            Points = 101;
            ScanType = ACAnalysisScan.Decade;
            Solver= new ACSweepSolver();
        }

    }
}
