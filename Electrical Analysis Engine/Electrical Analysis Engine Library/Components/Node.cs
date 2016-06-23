using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Numerics;

namespace ElectricalAnalysis.Components
{
    public abstract class Node: Item
    {

        [Browsable(false)]
        public ComponentContainer Owner { get; set; }


        [Browsable(false)]
        public List<Dipole> Components { get; protected set; }

        //Circuit parent
        public Node(string name = null)
            : base()
        {
            Components = new List<Dipole>();
            NameThis(name);
        }

        protected override string DefaultName { get { return "Node"; } }

        protected virtual void NameThis(string name)
        {
            if (string.IsNullOrEmpty(name))
                Name = DefaultName + ID.ToString();
            else
                Name = name;
        }

        public virtual void Reset()
        {
            //Voltage = Complex.Zero;
        }

       
    }
}
