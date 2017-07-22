using CircuitMVVMBase.Commands;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CircuitMVVMBase.MVVM.ViewModel
{
    public abstract class Plotter2DViewModel:DrawableVMBase
    {

    
        protected CursorCoordinateGraph mouseTrack;

        bool showtrack;
        public bool ShowTrack
        {
            get { return showtrack; }
            set {
                if (RaisePropertyChanged(value, ref showtrack))
                {
                    //return;
                    OnShowTrack();
                }
            }
        }

        protected virtual void OnShowTrack()
        {
           //mouseTrack.
        }

        RelayCommand storageplottcmd, deletedplotcmd, showplotcmd, clearplotscmd;

        ObservableCollection<Tuple<Plotter2D, LineGraph>> plotteditems;
        /// <summary>
        /// Allow storage plots for use in near future 
        /// </summary>
        [Browsable(false)]
        public ObservableCollection<Tuple<Plotter2D, LineGraph>> PlottedItems {
            get { return plotteditems ?? (plotteditems = new ObservableCollection<Tuple<Plotter2D, LineGraph>>()); }
        }

        LineGraph selectedplot;
        /// <summary>
        /// Selected line plotted in 2D Plot
        /// </summary>
        [Browsable(false)]
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

        #region Commands
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


        public RelayCommand ShowPlottCommand
        {
            get
            {
                return showplotcmd ?? (showplotcmd = new RelayCommand(ShowPlot,
                        (x) => { return PlottedItems != null && SelectedPlot != null; }));
            }
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

        #endregion

        public Plotter2DViewModel()
        {
            mouseTrack = new CursorCoordinateGraph();
        }


        /// <summary>
        /// Delete all storaged plots
        /// </summary>
        /// <param name="obj"></param>
        public virtual void ClearPlots(object obj)
        {
            PlottedItems.Clear();
        }

        /// <summary>
        /// Redraw selected previous storage plot 
        /// </summary>
        /// <param name="obj"></param>
        public abstract void ShowPlot(object obj);

        /// <summary>
        /// Allow storage some plot for redraw it lately
        /// </summary>
        /// <param name="obj"></param>
        public abstract void StoragePlot(object obj);
        
        /// <summary>
        /// Delete selected plot from storage
        /// </summary>
        /// <param name="obj"></param>
        public abstract void DeletePlot(object obj);
        
    }
}
