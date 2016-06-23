using ElectricalAnalysis.Analysis.Solver;

namespace ElectricalAnalysis.Analysis
{
    public class TransientAnalysis:BasicAnalysis
    {
        public string Step { get; set; }
        public string FinalTime { get; set; }
        protected override string DefaultName { get { return "Transient Analysis"; } }

        public TransientAnalysis()
            : base()
        {
            Step = "100u";
            FinalTime = "10m";
            Solver = new TransientSolver();
        }

        public override object Clone()
        {
            TransientAnalysis clon = new TransientAnalysis();
            clon.Step = Step;
            clon.FinalTime = FinalTime;

            return clon;
        }
    }
}
