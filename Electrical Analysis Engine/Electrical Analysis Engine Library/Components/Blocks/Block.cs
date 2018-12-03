using System.Collections.Generic;

namespace ElectricalAnalysis.Components
{

    /// <summary>
    /// Represent a generic block of conected components
    /// </summary>
    public abstract class Block: Dipole, ComponentContainer
    {
        public List<Dipole> Components { get; protected set; }
        //public List<Node> ExternalNodes { get; protected set; }


        public Block(ComponentContainer owner) : base(owner)
        {
            Components = new List<Dipole>();
        }

        

        public override void Reset()
        {
            foreach (var item in Components)
            {
                item.Reset();
            }
        }



    }
}
