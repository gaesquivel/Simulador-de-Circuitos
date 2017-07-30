using System;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;

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

        public override bool Parse(Circuit owner, string details)
        {
            string[] anali = details.ToLower().Substring(2).Split(" ".ToCharArray(),
                                            StringSplitOptions.RemoveEmptyEntries);

            TransientAnalysis setup = null;

            foreach (var itm in owner.Setup)
            {
                if (itm is TransientAnalysis)
                {
                    setup = itm as TransientAnalysis;
                    break;
                }
            }
            if (setup == null)
            {
                setup = new TransientAnalysis();
                owner.Setup.Add(setup);
            }
            if (anali.Length == 2)  //.tran 2m
                setup.FinalTime = anali[1];
            else if (anali.Length == 3) //.tran 2m startup
            {
                setup.FinalTime = anali[1];
                setup.Step = anali[2];
                //throw new NotImplementedException();
            }
            else if (anali.Length == 5) //.tran 0 2m 0 10u
            {
                setup.FinalTime = anali[2];
                setup.Step = anali[4];
            }
            else
            {
                NotificationsVM.Instance.Notifications.Add(
                   new Notification("Unknown analisys setup", Notification.ErrorType.warning));
                return false;
            }
            return true;
        }
    }
}
