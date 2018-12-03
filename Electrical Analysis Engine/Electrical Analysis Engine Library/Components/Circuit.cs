using System;
using System.Collections.Generic;
using System.IO;
using ElectricalAnalysis.Components.Generators;
using ElectricalAnalysis.Components.Controlled;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;
using System.Windows;
using ElectricalAnalysis.Analysis;
using ElectricalAnalysis.Components.Semiconductors.Diode;

namespace ElectricalAnalysis.Components
{
    /// <summary>
    /// Represent a Electric Circuit, including all pasive
    /// and active components (like resistors)
    /// </summary>
    public class Circuit:Block, ICloneable//, ComponentContainer
    {
        public enum CircuitState
        { 
            Empty, FileLoaded, Optimized, Solving, Solved
        }

        private string circuitfilename = "", circuittxt = "";
        private bool haserrors;

        public Circuit OriginalCircuit { get; internal set; }

        bool parametricenable;
        /// <summary>
        /// Enable when parametric statements is found 
        /// </summary>
        public bool ParametricEnable
        {
            get { return parametricenable; }
            set { RaisePropertyChanged(value, ref parametricenable); } 
        }

        /// <summary>
        /// Native circuit is a text... this is that circuit
        /// </summary>
        public string CircuitText {
            get { return circuittxt; }
            set { RaisePropertyChanged(value, ref circuittxt); }
        }

        /// <summary>
        /// Name of current Circuit File archive
        /// </summary>
        public string FileName {
            get { return circuitfilename; }
            protected set { RaisePropertyChanged(value, ref circuitfilename); }
        }
        public CircuitState State { get; internal set; }

        /// <summary>
        /// Contains all desired analisys for te current circuit
        /// </summary>
        public List<BasicAnalysis> Setup { get; protected set; }

        //public List<Dipole> Components { get; protected set; }
        public new Dictionary<string, NodeSingle> Nodes { get; protected set; }

        /// <summary>
        /// Is true if some error was encountered for example in parsing
        /// </summary>
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
            : base(null)
        {
            //Components = new List<Dipole>();
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
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("File Not Found!", Notification.ErrorType.error));
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

                string[] lines = CircuitText.Replace('\t', ' ').Split("\r\n".ToCharArray(), 
                                                        StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    #region formating

                    string item = lines[i];
                  

                    if (item.StartsWith("*."))
                    {
                        #region alguna opcion de analisis
                        string[] anali = item.ToLower().Substring(2).Split(" ".ToCharArray(), 
                                                        StringSplitOptions.RemoveEmptyEntries);
                        switch (anali[0])
                        {
                            case "tran":
                                TransientAnalysis tran = new TransientAnalysis();
                                if (!tran.Parse(this, item))
                                    NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Invalid analisys setup", 
                                                        Notification.ErrorType.warning));
                                break;
                            case "op":
                                throw new NotImplementedException();
                                //break;
                            case "ac":
                                ACAnalysis ac = new ACAnalysis();
                                if (!ac.Parse(this, item))
                                    NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Invalid analisys setup", 
                                                        Notification.ErrorType.warning));
                                break;
                            case "acplain":
                                ComplexPlainAnalysis plain = new ComplexPlainAnalysis();
                                if (!plain.Parse(this, item))
                                    NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Invalid analisys setup",
                                                        Notification.ErrorType.warning));
                                break;
                            case "param":
                                ParametricAnalisys param = new ParametricAnalisys();
                                if (!param.Parse(this, item))
                                    NotificationsVM.Instance.Notifications.Add(
                                        new Notification("Invalid analisys setup",
                                                        Notification.ErrorType.warning));
                                else
                                    ParametricEnable = true;
                                break;
                            default:
                                break;
                        }
                        continue;
                        #endregion
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
                    Dipole comp = null;
                    string[] comp1 = elemn[0].Split("_".ToCharArray());

                    #endregion 

                    switch (comp1[0].ToUpper())
                    {
                        case "R":
                            comp = new Resistor(this, comp1[1], elemn[3]);
                            break;

                        case "V":
                            comp = ((Generator)comp).Parse(this, item);
                            if (comp == null)
                                return false;
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
                        case "D":
                            comp = new Diode(this);
                            break;

                        case "C":
                            comp = new Capacitor(this, comp1[1], elemn[3]);
                            break;

                        case "E":
                            VoltageControlledGenerator E;
                            NodeSingle node1 = null;
                            //E_LAPLACE1 out 0 LAPLACE {s} { (1) / (-100 + s)}
                            if (elemn[3].ToUpper() == "LAPLACE")
                            {
                                var L = new LaplaceVoltageGenerator(this, comp1[1]);
                                string exp = item.Substring(item.IndexOf('{'));
                                L.Expresion = exp;
                                E = L;
                                //if (!ValidateDuplicatedComponent(comp))
                                //    return false;

                                //Components.Add(comp);
                                //continue;
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

                    if (!ValidateDuplicatedComponent(comp))
                        return false;

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

        /// <summary>
        /// Find for the already exitance of the component
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        private bool ValidateDuplicatedComponent(Dipole comp)
        {
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

            ParametricAnalisys param = null;
            foreach (var analisis in Setup)
            {
                if (analisis is ParametricAnalisys)
                {
                    param = analisis as ParametricAnalisys;
                    break;
                }
            }

            bool state = true;
            if (param == null)
            {
                if (analisys == null)
                    foreach (var analisis in Setup)
                    {
                        Reset();
                        state &= analisis.Solver.Solve(this, analisis);
                    }
                else
                {
                    Reset();
                    state &= analisys.Solver.Solve(this, analisys);
                }
            }
            else
            {
                state &= param.Solver.Solve(this, param);
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
        public override void Reset()
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
