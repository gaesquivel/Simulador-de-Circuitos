using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using PotatoMathica;
using System.Windows;

namespace ElectricalAnalysis
{
    public abstract class Item
    {
        protected static Random rnd = new Random();

        [Browsable(false)]
        public int ID { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public Point Position { get; set; }     //es posicion absoluta? relativa al parent?
        //public Item Parent { get; protected set; }
        protected  static int itemcounter;

        public Item()
        {
            ID = itemcounter;// rnd.Next(100);             //deberia ser un autoincremental por tipo de componente, por ejemplo R1, R2...
            itemcounter++;
            Name = "Some" + ID.ToString();
            Description = "";
            //Position = new Point(40, 30);
        }


    }
}
