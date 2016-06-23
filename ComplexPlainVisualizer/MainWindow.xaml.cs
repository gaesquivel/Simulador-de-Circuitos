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
        MasterViewModel model;
  
        public MainWindow()
        {
            InitializeComponent();

            model = plano;//new MasterViewModel();
            //model.
            //RegisterName()
            model.ComplexVM.surface = surface1;
            model.ComplexVM.ViewPort = viewport.Viewport;

            model.BodeVM.phasegraph = phasegraph;
            model.BodeVM.linegraph = linegraph;
            model.BodeVM.ModulePlotter = plotter;
            model.BodeVM.PhasePlotter = otherPlotter;

            model.TransientVM.Plotter = tplotter;
            model.TransientVM.linegraph = tlinegraph;

            DataContext = model;
        }
     
    }
}
