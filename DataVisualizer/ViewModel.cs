using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataVisualizer
{
    // Create a ViewModel
    public class ViewModel
    {
        public ObservableCollection<Point> Collection { get; set; }
        public ViewModel()
        {
            Collection = new ObservableCollection<Point>();
            GenerateDatas();
        }
        private void GenerateDatas()
        {
            this.Collection.Add(new Point(0, 1));
            this.Collection.Add(new Point(1, 2));
            this.Collection.Add(new Point(2, 3));
            this.Collection.Add(new Point(3, 4));
        }
    }
}
