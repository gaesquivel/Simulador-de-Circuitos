using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComplexPlainVisualizer
{
    /// <summary>
    /// Lógica de interacción para WPFParametric.xaml
    /// </summary>
    public partial class WPFParametric : Window
    {
        public WPFParametric()
        {
            InitializeComponent();

            plano.ComplexVM.surface = surface1;
            plano.ComplexVM.ViewPort = viewport.Viewport;

            plano.RecentFiles = RecentFileList;

            plano.BodeVM.phasegraph = phasegraph;
            plano.BodeVM.linegraph = linegraph;
            plano.BodeVM.ModulePlotter = bodemoduleplotter;
            plano.BodeVM.PhasePlotter = bodephasePlotter;

            plano.TransientVM.Plotter = timeplotter;
            plano.TransientVM.linegraph = timelinegraph;

            plano.FourierVM.SpectrumPlotter = fftplotter;
            plano.FourierVM.FFTModuleGraph = fftlinegraph;
            plano.FourierVM.FFTCosineGraph = fftrealgraph;
            plano.FourierVM.FFTSineGraph = fftimaggraph;
            plano.FourierVM.WindowPlotter = fftwindow;
            plano.FourierVM.Windowlinegraph = fftwindowlinegraph;

            plano.InverseFourierVM.SpectrumPlotter = fftplotter;
            plano.InverseFourierVM.WindowPlotter = fftwindow;
        }
    }
}
