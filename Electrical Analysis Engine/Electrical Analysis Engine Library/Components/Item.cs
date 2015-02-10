using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PotatoMathica;
using System.Windows;

namespace ElectricalAnalysis
{
    public abstract class Item
    {
        protected static Random rnd = new Random();

        public int ID { get; protected set; }
        public string Name { get; set; }
        //public Point Position { get; set; }     //es posicion absoluta? relativa al parent?
        //public Item Parent { get; protected set; }


        public Item()
        {
            ID = rnd.Next(100);             //deberia ser un autoincremental por tipo de componente, por ejemplo R1, R2...
            Name = "Zaraza" + ID.ToString();
            //Position = new Point(40, 30);
        }


    }
}
