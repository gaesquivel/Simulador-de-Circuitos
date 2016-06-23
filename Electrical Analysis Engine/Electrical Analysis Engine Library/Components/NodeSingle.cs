using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components
{
    public class NodeSingle:Node
    {
        private NodeType typeofnode;

        public enum NodeType
        {
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
            /// Node where can apply Norton Theorem
            /// </summary>
            NortonSingleNode,

            /// <summary>
            /// This node is member of supernode
            /// </summary>
            SuperNodeMember,

            /// <summary>
            /// Node where the voltage is well known
            /// </summary>
            VoltageFixedNode,

            /// <summary>
            /// Node where voltage depends of other variable
            /// such as voltage controlled generator
            /// </summary>
            VoltageDependentNode
        }

        [Browsable(false)]
        public NodeType TypeOfNode
        {
            get { return typeofnode; }
            set
            {
                if (value < typeofnode || IsReference)
                    return;
                typeofnode = value;
            }
        }

        public Complex Voltage { get; internal set; }

        [Browsable(false)]
        public bool IsReference { get; set; }

        public NodeSingle(string name = null):base(name)
        {


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


        public override void Reset()
        {
            Voltage = Complex.Zero;
            typeofnode = NodeType.Unknow;
        }


        public override string ToString()
        {
            return "Node " + Name + ", Voltage " + Voltage.ToString();
        }

    }
}
