using DataVisualizer.MVVM.ViewModel;
using System.Windows;

namespace DataVisualizer
{
    /// <summary>
    /// Interaction logic for TransientPlotter.xaml
    /// </summary>
    public partial class TransientPlotter : Window
    {
        TransientViewModel model { get; set; }

        public TransientPlotter()
        {
            InitializeComponent();

            model = plano;//new TransientViewModel();
            model.Plotter = plotter;
            model.linegraph = linegraph;
            DataContext = model;

        }
    }
}
