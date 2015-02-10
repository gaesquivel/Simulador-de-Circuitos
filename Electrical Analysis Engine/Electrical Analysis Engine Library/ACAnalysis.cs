using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public class ACAnalysis:Analysis
    {
        enum ACAnalysisScan { Decade, Linear }


        public string StartFrequency { get; set; }
        public string EndFrequency { get; set; }
        public int Points{ get; set; }
        public ACAnalysisScan ScanType { get; set; }

        public ACAnalysis():base()
        {
            StartFrequency = "10";
            EndFrequency = "10Meg";
            Points = 101;
            ScanType = ACAnalysisScan.Decade;
        }

    }
}
