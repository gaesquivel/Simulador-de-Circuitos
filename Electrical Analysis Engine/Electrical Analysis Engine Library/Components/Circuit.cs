using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace ElectricalAnalysis.Components
{
    public class Circuit:Item, ICloneable, ComponentContainer
    {

        public List<BasicAnalysis> Setup { get; protected set; }
        public List<Dipole> Components { get; protected set; }
        public Dictionary<string,Node> Nodes { get; protected set; }
        public Boolean HasErrors { get; protected set; }
        public Node Reference { get; set; }

        public double CircuitTime { get; set; }

        //public Vector<double> StaticResult;
        //public Vector<double> StaticVector;
        //public Matrix<double> StaticMatrix;

        public Circuit()
            : base()
        {
            Name = "Circuit" + ID.ToString();
            Components = new List<Dipole>();
            Nodes = new Dictionary<string,Node>();
            Setup = new List<BasicAnalysis>();
            Setup.Add(new DCAnalysis());
        }

        /// <summary>
        /// Read a netlist file (.net) containing componentsdescription y nodes
        /// A Circuit object with components y nodes will be created 
        /// </summary>
        /// <param name="CircuitName"></param>
        public void ReadCircuit(string CircuitName)
        {
            if (!File.Exists(CircuitName))
            {
                HasErrors = true;
                throw new FileNotFoundException();
            }
            try
            {
                TextReader reader = File.OpenText(CircuitName);

                string txt = reader.ReadToEnd();
                reader.Close();

                string[] lines = txt.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in lines)
                {
                    //un comentario
                    if (item.StartsWith("*"))
                        continue;

                    //R_R1         $N_0002 $N_0001  1k
                    string[] elemn = item.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    ElectricComponent comp = null;
                    string[] comp1 = elemn[0].Split("_".ToCharArray());
                    switch (comp1[0].ToUpper())
                    {
                        case "R":
                            comp = new Resistor(this, comp1[1], elemn[3]);
                            break;
                        case "V":
                            if (elemn.Length == 4)
                                comp = new VoltageGenerator(this, comp1[1], elemn[3]);
                            else if (elemn.Length == 7 || elemn.Length == 8)
                            {
                                ACVoltageGenerator ac = new ACVoltageGenerator(this, comp1[1], elemn[4]);
                                if (elemn.Length == 8)
                                    ac.ACVoltage = new Complex32((float)StringUtils.DecodeString(elemn[6]), 
                                                                (float)StringUtils.DecodeString(elemn[6]));
                                else
                                    ac.ACVoltage = new Complex32((float)StringUtils.DecodeString(elemn[6]), 0);
                                comp = (ACVoltageGenerator)ac;
                            }
                            break;
                        case "I":
                            if (elemn[3] == "DC")
                                comp = new CurrentGenerator(this, comp1[1], elemn[4]);
                            else
                                //aun sin resolver para otros generadores
                                comp = new CurrentGenerator(this, comp1[1], elemn[4]);

                            break;
                        case "L":
                            comp = new Inductor(this, comp1[1], elemn[3]);

                            break;
                        case "C":
                            comp = new Capacitor(this, comp1[1], elemn[3]);

                            break;

                        default:
                            throw new Exception();
                    }

                    comp.Nodes.Clear();
                   //agrego los nodos al circuito y al componente
                    Node n;
                    if (!Nodes.ContainsKey(elemn[1]))
                    {
                        n = new Node(elemn[1]);
                        Nodes.Add(n.Name, n);
                    }
                    else
                        n = Nodes[elemn[1]];

                    comp.Nodes.Add(n);
                    n.Components.Add(comp);
                    if (n.Name == "0")
                    {
                        n.IsReference = true;
                        Reference = n;
                    }
                    //agrego el segundo nodo
                    if (!Nodes.ContainsKey(elemn[2]))
                    {
                        n = new Node(elemn[2]);
                        Nodes.Add(elemn[2], n);
                    }
                    else
                        n = Nodes[elemn[2]];

                    comp.Nodes.Add(n);
                    n.Components.Add(comp);
                   
                    //if (n.Name == "0")
                    //    n.IsReference = true;

                    Components.Add(comp);
                }
            }
            catch (Exception ex)
            {
                HasErrors = true;
                throw;
            }

           // return cir;
        }

        public string Solve()
        { 
            string file = "";

            foreach (var item in Setup)
            {
                item.Solver.Solve(this, item);
            }

            return file;
        }

        public void Reset()
        {
            foreach (var item in Components)
            {
                item.Reset();
            }
            foreach (var item in Nodes.Values)
            {
                item.Reset();
            }
        }

        public object Clone()
        {
            Circuit cir = new Circuit();
            foreach (var node in Nodes)
            {
                cir.Nodes.Add(node.Key, node.Value);
            }
            cir.Components.AddRange(Components);
            cir.Reference = Reference;

            return cir;
        }
    }
}
