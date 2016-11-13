using CircuitMVVMBase.MVVM;
using DataVisualizer.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComplexPlainVisualizer.MVVM.ViewModel
{
    public class MasterViewModel:ViewModelBase
    {
        public ComplexPlainViewModel ComplexVM { get; set; }
        public BodeViewModel BodeVM { get; set; }
        public TransientViewModel TransientVM { get; set; }
        public DCViewModel DCVM { get; set; }

        

        public ObservableCollection<ViewModelBase> Models { get; private set; }

        public Visibility PlotterToolBarVisible
        {
            get {
                if (selectedvm != null && selectedvm is Plotter2DViewModel)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        ViewModelBase selectedvm;
        public ViewModelBase SelectedVM {
            get { return selectedvm; }
            set {
                RaisePropertyChanged(value, ref selectedvm);
                RaisePropertyChanged("PlotterToolBarVisible");
                }
        }

        public MasterViewModel()
        {
            BodeVM = new BodeViewModel();
            ComplexVM = new ComplexPlainViewModel();
            TransientVM = new TransientViewModel();
            Models = new ObservableCollection<ViewModelBase>();
            Models.Add(ComplexVM);
            Models.Add(BodeVM);
            Models.Add(TransientVM);
            //Models.Add(DCVM);
            SelectedVM = ComplexVM;
        }

    }
}
