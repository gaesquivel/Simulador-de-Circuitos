using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase.MVVM.ViewModel;
using DataVisualizer.MVVM.ViewModel;
using ElectricalAnalysis;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace ComplexPlainVisualizer.MVVM.ViewModel
{
    public class ParametricViewModel : CircuitSimulationViewModel
    {
        public ComplexPlainViewModel ComplexVM { get; set; }
        public BodeViewModel BodeVM { get; set; }
        public TransientViewModel TransientVM { get; set; }
        public DCViewModel DCVM { get; set; }
        public FourierViewModel FourierVM { get; set; }
        public InverseFourierViewModel InverseFourierVM { get; set; }



        public ObservableCollection<ViewModelBase> Models { get; private set; }

        public Visibility PlotterToolBarVisible
        {
            get
            {
                if (selectedvm != null && selectedvm is Plotter2DViewModel)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        ViewModelBase selectedvm;
        public ViewModelBase SelectedVM
        {
            get { return selectedvm; }
            set
            {
                RaisePropertyChanged(value, ref selectedvm);
                RaisePropertyChanged("PlotterToolBarVisible");
                SelectedObject = value;
            }
        }

        public ParametricViewModel()
        {
            BodeVM = new BodeViewModel();
            ComplexVM = new ComplexPlainViewModel();
            TransientVM = new TransientViewModel();
            FourierVM = new FourierViewModel();
            InverseFourierVM = new InverseFourierViewModel();
            
            Models = new ObservableCollection<ViewModelBase>();
            Models.Add(ComplexVM);
            Models.Add(BodeVM);
            Models.Add(TransientVM);
            Models.Add(FourierVM);
            Models.Add(InverseFourierVM);

            FourierVM.transientVM = TransientVM;
            InverseFourierVM.FFTViewModel = FourierVM;
            SelectedVM = ComplexVM;
        }


        #region plot section
        public override void DeletePlot(object obj)
        {
            throw new NotImplementedException();
        }

        public override void ShowPlot(object obj)
        {
            throw new NotImplementedException();
        }

        public override void StoragePlot(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override void Simulate(object obj)
        {
            if (CurrentCircuit == null || obj is string)
            {
                CurrentCircuit = new Circuit();
            }
            if (obj is string)
            {
                string file = obj as string;
                CurrentCircuit.ReadCircuit(file);
            }

            if (CurrentCircuit.ParametricEnable)
            {
                ParametricAnalisys panali = null;
                foreach (var anali in CurrentCircuit.Setup)
                {
                    if (anali is ParametricAnalisys)
                    {
                        panali = anali as ParametricAnalisys;
                        break;
                    }
                }

                if (panali == null)
                {
                    NotificationsVM.Instance.Notifications.Add(
                        new Notification("Parametric Analisys not found!", Notification.ErrorType.error));
                    return;
                }

                if (!panali.Solver.Solve(CurrentCircuit, panali))
                {
                    NotificationsVM.Instance.Notifications.Add(
                        new Notification("An error was founded in Parametric simulation!", Notification.ErrorType.error));
                    return;
                }

                bool firstt = true, firstb = true;
                ViewModelBase last = null;
                foreach (var item in ((ParametricSolver)panali.Solver).Voltages)
                {
                    if (item.Item1 is TransientAnalysis)
                    {
                        if (firstt)
                        {
                            TransientVM.ClearPlots(null);
                            firstt = false;
                        }
                        TransientVM.Redraw(item);
                        last = TransientVM;
                    }
                    else if (item.Item1 is ACAnalysis)
                    {
                        if (firstb)
                        {
                            BodeVM.ClearPlots(null);
                            firstb = false;
                        }
                        BodeVM.Redraw(item);
                        last = BodeVM;
                    }
                }

                SelectedVM = last;

            }




        }

        public override void Redraw(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
