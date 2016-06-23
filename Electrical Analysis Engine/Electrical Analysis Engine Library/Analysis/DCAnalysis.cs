using ElectricalAnalysis.Analysis.Solver;

namespace ElectricalAnalysis
{
    public class DCAnalysis: BasicAnalysis
    {
        protected override string DefaultName { get { return "DC Bias Analysis"; } }

        public DCAnalysis()
            : base()
        {

            Solver = new DCSolver();
        }

        public override object Clone()
        {
            return new DCAnalysis();
        }

    }
}
