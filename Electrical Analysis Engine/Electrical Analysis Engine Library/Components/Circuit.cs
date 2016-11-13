using System;
using System.Collections.Generic;
using System.IO;
using ElectricalAnalysis.Components.Generators;
using ElectricalAnalysis.Components.Controlled;
using System.Numerics;
using System.ComponentModel;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;
using System.Windows;
using System.Threading.Tasks;
using ElectricalAnalysis.Analysis;

namespace ElectricalAnalysis.Components
{
    /// <summary>
    /// Represent a Electric Circuit, including all pasive
    /// and active components (like resistors)
    /// </summary>
    public class Circuit:Item, ICloneable, ComponentContainer
    {
        public enum CircuitState
        { 
            Empty, FileLoaded, Optimized, Solving, Solved
        }

        private string circuitfilename = "", circuittxt = "";
        private bool haserrors;

        public Circuit OriginalCircuit { get; internal set; }


        public string CircuitText {
            get { return circuittxt; }
            set { RaisePropertyChanged(value, ref circuittxt); }
        }

        public string FileName {
            get { return circuitfilename; }
            protected set { RaisePropertyChanged(value, ref circuitfilename); }
        }
        public CircuitState State { get; internal set; }
        public List<BasicAnalysis> Setup { get; protected set; }
        public List<Dipole> Components { get; protected set; }
        public Dictionary<string, NodeSingle> Nodes { get; protected set; }
        public bool HasErrors {
            get { return haserrors; }
            protected set { RaisePropertyChanged(value, ref haserrors); }
        }

        public bool IsChanged
        {
            get { return OriginalCircuit != null && CircuitText != OriginalCircuit.CircuitText; }
        }

        public NodeSingle Reference { get; protected set; }
        protected override string DefaultName { get { return "Circuit"; } }
        public double CircuitTime { get; set; }

        public Circuit()
            : base()
        {
            Components = new List<Dipole>();
            Nodes = new Dictionary<string, NodeSingle>();
            Setup = new List<BasicAnalysis>();
            Setup.Add(new DCAnalysis());
        }

        /// <summary>
        /// Read a netlist file (.net) containing components description y nodes
        /// A Circuit object with components y nodes will be created 
        /// </summary>
        /// <param name="CircuitName"></param>
        public void ReadCircuit(string CircuitName)
        {
            if (!File.Exists(CircuitName))
            {
                HasErrors = true;
                NotificationsVM.Instance.Notifications.Add(new Notification("File Not Found!", Notification.ErrorType.error));
                return;
            }
            try
            {
                HasErrors = false;
                FileName = CircuitName;
                TextReader reader = File.OpenText(CircuitName);

                CircuitText = reader.ReadToEnd();
                reader.Close();
                HasErrors = !Parse();
                if (HasErrors)
                    return;

                State = CircuitState.FileLoaded;
                OriginalCircuit = Clone() as Circuit;
            }
            catch (Exception ex)
            {
                HasErrors = true;
                NotificationsVM.Instance.Notifications.Add(new Notification(ex));
            }
        }

        /// <summary>
        /// Try to parse the current Circuit text
        /// </summary>
        /// <returns>Return false if parse fails</returns>
        public virtual bool Parse()
        {
            HasErrors = false;
            try
            {
                Components.Clear();
                Nodes.Clear();

                string[] lines = CircuitText.Replace('\t', ' ').Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    #region formating

                    string item = lines[i];
                  

                    if (item.StartsWith("*."))
                    {
                        //alguna opcion de analisis
                        string [] anali = item.ToLower().Substring(2).Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        switch (anali[0])
                        {
                            case "tran":
                                #region transient
                                TransientAnalysis setup = null;

                                foreach (var itm in Setup)
                                {
                                    if (itm is TransientAnalysis)
                                    {
                                        setup = itm as TransientAnalysis;
                                        break;
                                    }
                                }
                                if (setup == null)
                                {
                                    setup = new TransientAnalysis();
                                    Setup.Add(setup);
                                }
                                if (anali.Length == 2)  //.tran 2m
                                    setup.FinalTime = anali[1];
                                else if (anali.Length == 3) //.tran 2m startup
                                {
                                    setup.FinalTime = anali[1];
                                    setup.Step = anali[2];
                                    //throw new NotImplementedException();
                                }
                                else if (anali.Length == 5) //.tran 0 2m 0 10u
                                {
                                    setup.FinalTime = anali[2];
                                    setup.Step = anali[4];
                                }
                                else
                                    NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Unknown analisys setup", Notification.ErrorType.warning));
                                break;
                            #endregion
                            case "op":
                                throw new NotImplementedException();
                                //break;
                            case "ac":
                                throw new NotImplementedException();
                            case "plain":
                                throw new NotImplementedException();

                            //break;
                            default:
                                break;
                        }
                        continue;
                    }


                    //un comentario
                    if (item.StartsWith("*"))
                    {
                        if (i == 0)     //el primer comentario quizas sea una descripcion
                            Description = lines[i];
                        continue;
                    }


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

                    #endregion 

                    switch (comp1[0].ToUpper())
                    {
                        case "R":
                            comp = new Resistor(this, comp1[1], elemn[3]);
                            break;

                        case "V":
                            if (elemn.Length == 4)
                                comp = new VoltageGenerator(this, comp1[1], elemn[3]);
                            else if (elemn.Length == 5)
                                comp = new VoltageGenerator(this, comp1[1], elemn[4]);
                            else if (elemn.Length == 7 || elemn.Length == 8)
                            {
                                #region AC
                                ACVoltageGenerator ac = new ACVoltageGenerator(this, comp1[1], elemn[4]);
                                double v1 = 0, v2 = 0;
                                if (!StringUtils.DecodeString(elemn[6], out v1))
                                {
                                    NotificationsVM.Instance.Notifications.Add(new Notification("Error to parse: " + elemn[6], Notification.ErrorType.error));
                                    return false;
                                }
                                if (elemn.Length == 8 &&  StringUtils.DecodeString(elemn[7], out v2))
                                    ac.ACVoltage = Complex.FromPolarCoordinates(v1, v2);
                                else
                                    ac.ACVoltage = new Complex(v1, 0);
                                comp = (ACVoltageGenerator)ac;
                                #endregion
                            }
                            //else if (elemn.Length > 8 && item.Contains("SIN"))
                            else if (item.ToUpper().Contains("SIN"))
                            {
                                #region Vsin
                                //ltspice
                                //V1 s 0 SINE(0 10 1k 10u) AC 1

                                //orcad
                                //V_V1  n1 n2 DC 0 AC 1
                                //+SIN 1 1 1k 0 0 0
                                SineVoltageGenerator vsin = new SineVoltageGenerator(this, comp1[1]);
                               
                                double val = 0;
                                if (StringUtils.DecodeString(elemn[6], out val))
                                    vsin.ACVoltage = new Complex(val, 0);
                                else
                                    NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[6], Notification.ErrorType.error));
                                if (StringUtils.DecodeString(elemn[4], out val))    //DC
                                    vsin.Value = val;
                                else
                                    NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[4], Notification.ErrorType.error));
                                vsin.Amplitude = elemn[9];
                                vsin.Offset = elemn[8];
                                vsin.Frequency = elemn[10];
                                vsin.Thau = elemn[12];
                                vsin.Delay = elemn[11];
                                vsin.Phase = elemn[13];
                                comp = vsin;
                                #endregion
                            }
                            else if (item.ToUpper().Contains("PULSE"))
                            {
                                #region V pulse

                                //ltSpice
                                //V1 s 0 PULSE(0 10 10u 1n 1n 500u 1m 6) AC 1

                                //Me
                                //V1 n1 n2 DC 0 AC 1 PULSE offset amplitude Frequency Ton Delay Trise Tfall Cicles
                                //Default     0    1         0       1           1K    50   0     0    0     oo
                                //V1 n1 n2 DC 0 AC 1 PULSE 0 10 10u 1n 1n 500u 1m 6
                                PulseVoltageGenerator vpul = new PulseVoltageGenerator(this, comp1[1]);
                                
                                double val = 0;
                                if (StringUtils.DecodeString(elemn[6], out val))
                                    vpul.ACVoltage = new Complex(val, 0);
                                else
                                    NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[6], Notification.ErrorType.error));
                                if (StringUtils.DecodeString(elemn[4], out val))    //DC
                                    vpul.Value = val;
                                else
                                    NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[4], Notification.ErrorType.error));
                                vpul.Offset = elemn[8];
                                vpul.Amplitude = elemn[9];
                                vpul.Frequency = elemn[10];
                                if (elemn.Length > 9)
                                {
                                    if (StringUtils.DecodeString(elemn[11], out val))    //DC
                                        vpul.OnTime = (float)val;
                                    else
                                        NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[11], Notification.ErrorType.error));
                                //vpul.OnTime = elemn[11];
                                    vpul.Delay = elemn[12];
                                    vpul.RiseTime = elemn[13];
                                    vpul.FallTime = elemn[14];
                                    vpul.Cicles = elemn[15];
                                }
                               
                                comp = vpul;

                                #endregion

                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            break;


                        case "I":
                            if (elemn[3] == "DC")
                                comp = new CurrentGenerator(this, comp1[1], elemn[4]);
                            else if(elemn.Length == 4)
                                comp = new CurrentGenerator(this, comp1[1], elemn[3]);
                            else
                                //aun sin resolver para otros generadores
                                throw new NotImplementedException();
                            //comp = new CurrentGenerator(this, comp1[1], elemn[4]);

                            break;

                        case "L":
                            comp = new Inductor(this, comp1[1], elemn[3]);
                            break;

                        case "C":
                            comp = new Capacitor(this, comp1[1], elemn[3]);
                            break;

                        case "E":
                            VoltageControlledGenerator E;
                            NodeSingle node1 = null;
                            //E_LAPLACE1 out 0 LAPLACE { V(s)} { (1) / (-100 + s)}
                            if (elemn[3].ToUpper() == "LAPLACE")
                            {
                                var L = new LaplaceVoltageGenerator(this, comp1[1]);
                                string exp = item.Substring(item.IndexOf('{'));
                                L.Expresion = exp;
                                E = L;
                                node1 = CreateOrFindNode("in+");
                                E.InputNodes.Add(node1);

                                node1 = CreateOrFindNode("in-");
                                E.InputNodes.Add(node1);
                            }
                            //E_E1         $N_0002 0 $N_0001 0 10
                            else //if (true)
                            {
                                E = new VoltageControlledGenerator(this, comp1[1]);
                                E.Gain = elemn[5];
                                node1 = CreateOrFindNode(elemn[3]);
                                E.InputNodes.Add(node1);

                                node1 = CreateOrFindNode(elemn[4]);
                                E.InputNodes.Add(node1);
                            }

                            comp = E;
                            break;

                        case "G":
                            //G_G1         $N_0002 0 $N_0001 0 10m
                            TransConductanceGenerator Gm = new TransConductanceGenerator(this, comp1[1]);
                            Gm.Gain = elemn[5];

                            NodeSingle node = null;
                            node = CreateOrFindNode(elemn[3]);
                            Gm.InputNodes.Add(node);

                            node = CreateOrFindNode(elemn[4]);
                            Gm.InputNodes.Add(node);

                            comp = Gm;
                            break;

                        default:
                            throw new Exception();
                    }

                    foreach (var com in Components)
                    {
                        if (com.Name == comp.Name)
                        {
                            NotificationsVM.Instance.Notifications.Add(
                                new Notification("Component already exist: " + com.Name, 
                                                    Notification.ErrorType.error));
                            return false;
                        }
                    }

                    #region creating nodes

                    comp.Nodes.Clear();
                    //agrego los nodos al circuito y al componente
                    NodeSingle n;
                    if (!Nodes.ContainsKey(elemn[1]))
                    {
                        n = new NodeSingle(elemn[1]);
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
                        n = new NodeSingle(elemn[2]);
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

                    #endregion

                    Components.Add(comp);
                }
            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(new Notification(ex));
                return false;
            }
            if (OriginalCircuit != null)
                OriginalCircuit.CircuitText = this.CircuitText;

            return true;
        }

        private NodeSingle CreateOrFindNode(string name)
        {
            NodeSingle n = null;
            if (Nodes.ContainsKey(name))
                n = Nodes[name];
            else
            {
                n = new NodeSingle(name);
                Nodes.Add(n.Name, n);
            }
            return n;
        }

        public bool Solve(BasicAnalysis analisys = null)
        {
            //if (State >= CircuitState.Optimized)
            //{
            //    OptimizedCircuit = this;
            //}

            switch (State)
            {
                case CircuitState.Empty:
                    //HasErrors = true;
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
            if (analisys == null)
                foreach (var analisis in Setup)
                {
                    Reset();
                    state &= analisis.Solver.Solve(this, analisis);
                }
            else
            {
                Reset();
                 analisys.Solver.Solve(this, analisys);
            }

            if (state)
                State = CircuitState.Solved;
            else
                HasErrors = true;
            return state;
        }

        /// <summary>
        /// Perform a full reset of all circuit components
        /// </summary>
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

        public override string ToString()
        {
            return CircuitText;
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
            cir.CircuitText = CircuitText;
            return cir;
        }

        public bool Save(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = FileName;
                //Notifications.Add(new Notification("Invalid Filename", Notification.ErrorType.error));
                //return false;
            }

            if (File.Exists(filename))
                if (MessageBox.Show("Realy do you want to rewrite the file?", "Rewrite?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return false;

            try
            {
                StreamWriter writer = File.CreateText(filename);
                writer.Write(CircuitText);
                writer.Close();
            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(new Notification(ex));
                return false;
            }
            OriginalCircuit = Clone() as Circuit;
            FileName = filename;
            return true;
        }
    }
}
