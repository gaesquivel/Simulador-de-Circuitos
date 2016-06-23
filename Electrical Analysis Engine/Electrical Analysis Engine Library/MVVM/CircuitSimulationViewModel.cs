using ElectricalAnalysis;
using ElectricalAnalysis.Components;
using System.ComponentModel;
using Microsoft.Win32;
using ElectricalAnalysis.Analysis.Solver;
using CircuitMVVMBase.Commands;
using System.Threading.Tasks;

namespace CircuitMVVMBase.MVVM.ViewModel
{
    public abstract class CircuitSimulationViewModel:DrawableVMBase
    {
        static Circuit cir;
        Item plotted;

        public CircuitSimulationViewModel():base()
        {
            SimulateCommand.CanExecuteTarget = (x) => { return cir != null; };

            DrawSelectedCommand.CanExecuteTarget = (x) => {
                return (cir != null && plotted != null);
            };
            ExportCommand.CanExecuteTarget = (x) =>
            {
                return cir != null && cir.State == Circuit.CircuitState.Solved;
            };
            SaveFileCommand.CanExecuteTarget = (x) =>
            {
                return cir != null && cir.OriginalCircuit != null && cir.OriginalCircuit.CircuitText != cir.CircuitText;
            };
            SaveAsFileCommand.CanExecuteTarget = (x) =>
            {
                return cir != null && cir.OriginalCircuit != null  && cir.OriginalCircuit.CircuitText != cir.CircuitText;
            };
        }

        [Browsable(false)]
        public override object SelectedObject
        {
            get
            {
                return base.SelectedObject;
            }
            set
            {
                base.SelectedObject = value;
                if (value is Dipole || value is NodeSingle)
                    PlottedItem = value as Item;
            }
        }

        [Browsable(false)]
        public Item PlottedItem
        {
            get { return plotted; }
            set { RaisePropertyChanged(value, ref plotted); }
        }

        [Browsable(false)]
        public Circuit CurrentCircuit
        {
            get { return cir; }
            set { RaisePropertyChanged(value, ref cir); }
        }

        protected virtual BasicAnalysis CurrentAnalisys()
        {
            foreach (var item in CurrentCircuit.Setup)
            {
                if (IsAnalisysType(item))
                    return item;
            }
            return null;
        }
        
        protected virtual bool IsAnalisysType(BasicAnalysis analis)
        {
            return analis is DCAnalysis;
        }

        public override void Export(object obj)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Excel Result files (*.csv)|*.csv|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                DCSolver sol5 = cir.Setup[0].Solver as DCSolver;
                sol5.Export(save.FileName);
            }
        }

        protected override void newfile(object obj)
        {
            CurrentCircuit = new Circuit();
            //CurrentCircuit.Components.Add()
        }

        protected override void SaveFile(object obj)
        {
            cir.SaveCircuit();
        }

        protected override void SaveAsFile(object obj)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); ;
            save.Filter = "Circuit Net List files (*.net)|*.net|All files (*.*)|*.*";
            if (save.ShowDialog() == true)
            {
                cir.SaveCircuit(save.FileName);
            }
        }

    }
}
