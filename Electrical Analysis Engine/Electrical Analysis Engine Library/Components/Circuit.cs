using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ElectricalAnalysis.Components.Generators;
using ElectricalAnalysis.Components.Controlled;

namespace ElectricalAnalysis.Components
{
    public class Circuit:Item, ICloneable, ComponentContainer
    {
        public enum CircuitState
        { 
            Empty, FileLoaded, Optimized, Solved
        }

        public Circuit OriginalCircuit { get; protected set; }
        public Circuit OptimizedCircuit { get; internal set; }

        public CircuitState State { get; set; }
        public List<BasicAnalysis> Setup { get; protected set; }
        public List<Dipole> Components { get; protected set; }
        public Dictionary<string,Node> Nodes { get; protected set; }
        public Boolean HasErrors { get; protected set; }
        public Node Reference { get; set; }

        public double CircuitTime { get; set; }

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
                for (int i = 0; i < lines.Length; i++ )
                {
                    string item = lines[i];
                    //un comentario
                    if (item.StartsWith("*"))
                        continue;

                    int j = i + 1;
                    while (j < lines.Length && lines[j].StartsWith("+"))
                    {
                        item += " " + lines[j].Substring(1);
                        j++;
                        i++;
                    }

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
                                    ac.ACVoltage = Complex32.FromPolarCoordinates((float)StringUtils.DecodeString(elemn[6]),
                                                                (float)StringUtils.DecodeString(elemn[7]));
                                else
                                    ac.ACVoltage = new Complex32((float)StringUtils.DecodeString(elemn[6]), 0);
                                comp = (ACVoltageGenerator)ac;
                            }
                            else if (elemn.Length > 8)
                            {
                                //V_V1         $N_0001 0 DC 0 AC 1
                                //+SIN 1 1 1k 0 0 0
                                SineVoltageGenerator vsin = new SineVoltageGenerator(this, comp1[1]);
                                //if (elemn.Length == 8)
                                    //vsin.ACVoltage = new Complex32((float)StringUtils.DecodeString(elemn[6]),
                                    //                            (float)StringUtils.DecodeString(elemn[6]));
                                //else
                                   vsin.ACVoltage = new Complex32((float)StringUtils.DecodeString(elemn[6]), 0);
                                vsin.Amplitude = elemn[9];
                                vsin.Offset = elemn[8];
                                vsin.Frequency = elemn[10];
                                vsin.Thau = elemn[12];
                                vsin.Delay = elemn[11];
                                vsin.Phase = elemn[13];
                                comp = vsin;
                            }
                            else
                            {
                                throw new NotImplementedException();
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
                        case "E":
                            //E_E1         $N_0002 0 $N_0001 0 10
                            VoltageControlledGenerator E = new VoltageControlledGenerator(this, comp1[1]);
                            E.Gain = elemn[5];

                            Node node1 = null;
                            node1 = CreateOrFindNode(elemn[3]);
                            E.InputNodes.Add(node1);

                            node1 = CreateOrFindNode(elemn[4]);
                            E.InputNodes.Add(node1);

                            comp = E;
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

                    if (n.Name == "0")
                    {
                        n.IsReference = true;
                        Reference = n;
                    }
                    Components.Add(comp);
                }
                State = CircuitState.FileLoaded;
                OriginalCircuit = this;
            }
            catch (Exception ex)
            {
                HasErrors = true;
                throw;
            }

        }

        private Node CreateOrFindNode(string name)
        {
            Node n = null;
            if (Nodes.ContainsKey(name))
                n = Nodes[name];
            else
            {
                n = new Node(name);
                Nodes.Add(n.Name, n);
            }
            return n;
        }

        public bool Solve()
        {
            if (State >= CircuitState.Optimized)
            {
                OptimizedCircuit = this;
            }

            switch (State)
            {
                case CircuitState.Empty:
                    HasErrors = true;
                    break;
                case CircuitState.FileLoaded:
                    break;
                case CircuitState.Optimized:

                    break;
                case CircuitState.Solved:
                    break;
                default:
                    break;
            }
            if (HasErrors)
                return false;

            bool state = true;
            foreach (var item in Setup)
            {
                state &= item.Solver.Solve(this, item);
            }
            if (state)
                State = CircuitState.Solved;
            else
                HasErrors = true;
            return state;
        }

        public void Reset()
        {
            HasErrors = false;
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
            foreach (var comp in cir.Components)
            {
                comp.OwnerCircuit = cir;
            }
            cir.Reference = Reference;
            cir.OriginalCircuit = this;
            return cir;
        }
    }
}
