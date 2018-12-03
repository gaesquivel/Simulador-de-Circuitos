using DataVisualizer.MVVM.ViewModel;
using System.Windows;

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class BodeViewer : Window
    {
        BodeViewModel model;
  
        public BodeViewer()
        {
            InitializeComponent();

            model = plano;//new BodeViewModel();
            //model.
            //RegisterName()
            model.phasegraph = phasegraph;
            model.linegraph = linegraph;
            model.ModulePlotter = plotter;
            model.PhasePlotter = otherPlotter;
            DataContext = model;
            //model.Simulate("Circuits/RCLpi.net");
            // model.InitializeGraph();
            //model.CurrentCircuit.Solve();
        }

    }
}
