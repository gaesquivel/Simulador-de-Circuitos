using CircuitDesigner.MVVM.ViewModel;
using System.Windows;

namespace CircuitDesigner
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class WPFCircuitDesigner: Window
    {
        public WPFCircuitDesigner()
        {
            InitializeComponent();

            //CircuitDesignerViewModel.Instance.DrawingSurface = theCanvas;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CircuitDesignerViewModel.Instance.OpenCircuit();
        }
    }
}
