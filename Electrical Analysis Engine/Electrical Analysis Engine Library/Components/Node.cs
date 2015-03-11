using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Numerics;
using MathNet.Numerics;
using System.Diagnostics;
using System.ComponentModel;

namespace ElectricalAnalysis.Components
{
    public class Node: Item
    {
        private NodeType typeofnode;

        public enum NodeType { 
            /// <summary>
            /// this node was not analyze yet
            /// </summary>
            Unknow,
            /// <summary>
            /// This node is aislated from the circuit
            /// </summary>
            BreakNode, 
            /// <summary>
            /// This node is connected to an floating component,
            /// a component linked by only one node to the circuit
            /// </summary>
            FloatingNode, 
            /// <summary>
            /// This node is part of branch
            /// </summary>
            InternalBranchNode, 
            /// <summary>
            /// Node where can apply Norton Theorem
            /// </summary>
            MultibranchCurrentNode,

            /// <summary>
            /// Node where voltage es directly dependent of 
            /// other floating node
            /// </summary>
            VoltageLinkedNode,
            /// <summary>
            /// In this node voltage can be calculated by 
            /// voltage pasive divider ecuation
            /// </summary>
            VoltageDivideNode,
            ///// <summary>
            ///// In this case the voltage can be calculated with some formulae
            ///// </summary>
            //VoltageVariableNode,
            /// <summary>
            /// Node where the voltage is well known
            /// </summary>
            VoltageFixedNode   
        }

        [Browsable(false)]
        public NodeType TypeOfNode
        {
            get
            { return typeofnode; }
            set
            {
                if (value < typeofnode)
                    return;
                typeofnode = value;
            }
        }

        [Browsable(false)]
        public ComponentContainer Owner { get; set; }

        public Complex32 Voltage { get; internal set; }

        [Browsable(false)]
        public bool IsReference { get; set; }

        [Browsable(false)]
        public List<Dipole> Components { get; protected set; }
        
        /// <summary>
        /// indica si el nodo esta conectado directamente a un generador de tension
        /// </summary>
        [Browsable(false)]
        public bool IsVoltageConnected
        {
            get {
                foreach (var item in Components)
                {
                    if (item is VoltageGenerator)
                        return true;
                }
                return false;
            }
        }


        //Circuit parent
        public Node(string name = null)
            : base()
        {
            Components = new List<Dipole>();
            if (string.IsNullOrEmpty(name))
                Name = "Node" + ID.ToString();
            else
                Name = name;
            //Parent = parent;
        }

        [DebuggerStepThrough]
        public Dipole OtherComponent(Dipole compo)
        {
            if (compo == Components[0])
                return Components[1];
            else
                return Components[0];
        }

        /// <summary>
        /// Node Voltage is well known for some reason (or not)
        /// </summary>
        [Browsable(false)]
        public bool IsVoltageKnowed
        {
            get
            {
                if (TypeOfNode == NodeType.VoltageFixedNode || IsReference)
                    return true;
                return false;
            }
        }

        public void Reset()
        {
            Voltage = Complex32.Zero;
        }

        public override string ToString()
        {
            return "Node " + Name + ", Voltage " + Voltage.ToString();
        }
    }
}
