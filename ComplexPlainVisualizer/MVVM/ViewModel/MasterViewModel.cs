using CircuitMVVMBase.MVVM;
using DataVisualizer.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexPlainVisualizer.MVVM.ViewModel
{
    public class MasterViewModel:ViewModelBase
    {
        public ComplexPlainViewModel ComplexVM { get; set; }
        public BodeViewModel BodeVM { get; set; }
        public TransientViewModel TransientVM { get; set; }

        public ObservableCollection<ViewModelBase> Models { get; private set; }

        ViewModelBase selectedvm;
        public ViewModelBase SelectedVM {
            get { return selectedvm; }
            set { RaisePropertyChanged(value, ref selectedvm); }
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
            SelectedVM = ComplexVM;
        }

    }
}
