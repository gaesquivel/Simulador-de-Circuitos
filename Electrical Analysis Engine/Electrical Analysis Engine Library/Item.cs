using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PotatoMathica;
using System.Windows;

namespace ElectricalAnalysis
{
    public class Item
    {

        public int ID { get; protected set; }
        public string Name { get; set; }
        public Point Position { get; set; }

        public Item()
        {
            Random rnd = new Random();
            ID = rnd.Next(100);
            Name = "Zaraza" + ID.ToString();
            Position = new Point(40, 30);
        }


    }
}
