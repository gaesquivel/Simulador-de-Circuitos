using CircuitMVVMBase.Commands;
using CircuitMVVMBase.MVVM.ViewModel;
using Microsoft.Research.DynamicDataDisplay;
using System;
using System.Collections.ObjectModel;

namespace DataVisualizer.MVVM.ViewModel
{
    public abstract class Plotter2DViewModel:CircuitSimulationViewModel
    {

        RelayCommand storageplottcmd, deletedplotcmd, showplotcmd, clearplotscmd;

        ObservableCollection<LineGraph> plotteditems;
        public ObservableCollection<LineGraph> PlottedItems {
            get { return plotteditems ?? (plotteditems = new ObservableCollection<LineGraph>()); }
        }

        LineGraph selectedplot;
        /// <summary>
        /// Selected line plotted in 2D Plot
        /// </summary>
        public LineGraph SelectedPlot
        {
            get { return selectedplot; }
            set
            {
                if (RaisePropertyChanged(value, ref selectedplot, true))
                {
                    SelectedObject = value;
                }
            }
        }

        public RelayCommand StoragePlottCommand
        {
            get
            {
                return storageplottcmd ?? (storageplottcmd = new RelayCommand(StoragePlot,
                        (x)=> { return PlottedItems != null; }));
            }
        }

        public RelayCommand ClearPlotsCommand
        {
            get
            {
                return clearplotscmd ?? (clearplotscmd = new RelayCommand(ClearPlots,
                        (x) => { return PlottedItems != null &&
                                        PlottedItems.Count > 0; }));
            }
        }

        protected virtual void ClearPlots(object obj)
        {
            PlottedItems.Clear();
        }

        public RelayCommand ShowPlottCommand
        {
            get
            {
                return showplotcmd ?? (showplotcmd = new RelayCommand(ShowPlot,
                        (x) => { return PlottedItems != null && SelectedPlot != null; }));
            }
        }

        protected virtual void ShowPlot(object obj)
        {
            throw new NotImplementedException();
        }

        protected virtual void StoragePlot(object obj)
        {
            throw new NotImplementedException();
        }

        public RelayCommand DeletePlottCommand
        {
            get
            {
                return deletedplotcmd ?? (deletedplotcmd = new RelayCommand(DeletePlot,
                        (x) => { return PlottedItems != null &&
                                        PlottedItems.Count > 0 &&
                                        SelectedPlot != null; }));
            }
        }

        protected virtual void DeletePlot(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
