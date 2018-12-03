using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System;
using System.ComponentModel;

namespace ElectricalAnalysis
{
    public abstract class Item: ViewModelBase
    {
        protected static Random rnd = new Random();

        [Browsable(false)]
        public int ID { get; protected set; }
        //public string Name { get; set; }
        public string Description { get; set; }
        protected  static int itemcounter;
        protected virtual string DefaultName { get { return "Some"; } }

        public Item()
        {
            ID = itemcounter;         //deberia ser un autoincremental por tipo de componente, por ejemplo R1, R2...
            itemcounter++;
            Name = DefaultName + ID.ToString();
            Description = "";
        }



    }
}
