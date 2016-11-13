using CircuitMVVMBase.MVVM;
using ComplexPlainVisualizer.MVVM.ViewModel;
using DataVisualizer.MVVM.ViewModel;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace DataVisualizer
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //MasterViewModel model;
  
        public MainWindow()
        {
            InitializeComponent();

            //model = plano;//new MasterViewModel();
                          // img1.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("Images/Errors/note.png", UriKind.Relative));

            plano.ComplexVM.surface = surface1;
            plano.ComplexVM.ViewPort = viewport.Viewport;
            plano.ComplexVM.RecentFiles = RecentFileList;
            plano.BodeVM.phasegraph = phasegraph;
            plano.BodeVM.linegraph = linegraph;
            plano.BodeVM.ModulePlotter = plotter;
            plano.BodeVM.PhasePlotter = otherPlotter;

            plano.TransientVM.Plotter = tplotter;
            plano.TransientVM.linegraph = tlinegraph;

            //DataContext = model;
        }

    
    }
}
