using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricalAnalysis.Components;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ElectricalAnalysis.Components.Controlled;
using CircuitMVVMBase.MVVM;
using CircuitMVVMBase;
using System.Diagnostics;
using ElectricalAnalysis.Analysis.Data;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class DCSolver : BasicSolver, CircuitSolver
    {
        public enum ExportFormats
        {
            CSV,
            Bitmap
        }

        public Circuit CurrentCircuit { get; protected set; }

       /* static int progress;
        public int Progress
        {
            get { return progress; }
            set {
                if (value >=0 && value <= 100)
                    RaisePropertyChanged(value, ref progress);
            }
        }*/

        /// <summary>
        /// Realize an analisys to find reelevant information to
        /// aids before to solve
        /// </summary>
        protected virtual SolveInfo PreAnalizeToSolve(Circuit cir)
        {
            CurrentCircuit = cir;
            SolveInfo solveinfo = new SolveInfo(cir);

            #region node analisys

            //Escaneo los nodos desde tierra para encontrar todos 
            //los nodos de tension fija
            ScanEarthSuperNode(cir.Reference, null);


            #region special component zone

            foreach (var comp in cir.Components)
            {
                if (comp is ControlledDipole)
                {
                    solveinfo.SpecialComponents.Add((ControlledDipole)comp);
                    if (comp is ControlledCurrentGenerator)
                        solveinfo.SpecialOutputCurrentComponents.Add(comp);
                    if (comp is ControllerShortCircuit)
                        solveinfo.SpecialInputCurrentComponents.Add(comp);

                    if (comp is ControlledVoltageGenerator)
                        foreach (var nodo in comp.Nodes)
                        {
                            if (nodo.IsReference)
                                continue;
                            nodo.TypeOfNode = NodeSingle.NodeType.VoltageDependentNode;
                            solveinfo.SpecialComponentNodes.Add(nodo);
                        }
                }
            }

            #endregion


            //aqui agrego los nodos de tension fija a la lista
            foreach (var nodo in cir.Nodes.Values)
            {
                if (nodo.IsReference)
                    continue;
                //los nodos fijosde componentes especiales son incognitas
                if  (solveinfo.SpecialComponentNodes.Contains(nodo))
                    continue;

                if (nodo.TypeOfNode == NodeSingle.NodeType.VoltageFixedNode)
                {
                    solveinfo.AutoCalculableNodes.Add(nodo);
                    continue;
                }
            }

            foreach (var nodo in cir.Nodes.Values)
            {
                if (nodo.IsReference)
                    continue;
            
                bool isnodesimple = true;
                foreach (var compo in nodo.Components)
                {
                    if (IsPartOfSuperNode(compo))
                    {
                        if (compo.IsConnectedToEarth)
                            if (compo is ControlledDipole)
                            {
                                continue;
                            }
                        isnodesimple = false;
                        break;
                    }
                }
                //si es tipo Unknow, no es VoltageFixedNode!
                if (nodo.TypeOfNode == NodeSingle.NodeType.Unknow)
                {
                    if (isnodesimple)
                    {
                        if (solveinfo.SpecialComponentNodes.Contains(nodo))
                            continue;
                        //es un nodo simple aislado
                        nodo.TypeOfNode = NodeSingle.NodeType.NortonSingleNode;
                        solveinfo.NortonNodes.Add(nodo);
                        solveinfo.Nodes.Add(nodo);
                    }
                    else
                    {
                        //es parte de un supernodo
                        SuperNode super = FindSuperNodeElements(nodo);
                        solveinfo.SuperNodes.Add(super);
                        solveinfo.Nodes.AddRange(super.Nodes);
                        //nodo pertenece a un supernodo
                    }
                }
            }

            #endregion

            foreach (var snode in solveinfo.SuperNodes)
            {
                solveinfo.NodesInSupernodes.AddRange(snode.Nodes);
                solveinfo.GeneratorInSupernodes.AddRange(snode.Components);
            }


            return solveinfo;
        }


        /// <summary>
        /// Given a node, find all other nodes and components inner supernode structure
        /// </summary>
        /// <param name="FirstNode"></param>
        protected virtual SuperNode FindSuperNodeElements(NodeSingle FirstNode, SuperNode super = null)
        {
            if (super == null)
                super = new SuperNode();

            try
            {
                super.Nodes.Add(FirstNode);
                NodeSingle nodo = FirstNode;
                //nodo.oe
                nodo.TypeOfNode = NodeSingle.NodeType.SuperNodeMember;

                foreach (var comp in nodo.Components)
                {
                    if (IsVoltageGenerator(comp))
                    {
                        if (super.Components.Contains(comp))
                            continue;

                        //agrego el componente 
                        super.Components.Add(comp);
                        if (comp.Nodes[0] == nodo)
                        {
                            if (!super.Nodes.Contains(comp.Nodes[1]))
                                FindSuperNodeElements(comp.Nodes[1], super);
                        }
                        else
                        {
                            if (!super.Nodes.Contains(comp.Nodes[0]))
                                FindSuperNodeElements(comp.Nodes[0], super);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(new Notification(ex));
            }
            return super;
        }


        /// <summary>
        /// Scan all nodes near reference node connected with
        /// Voltage generators, finding then earth supernode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="previos"></param>
        private void ScanEarthSuperNode(NodeSingle node, NodeSingle previos)
        {
            foreach (var comp in node.Components)
            {
                if (IsVoltageGenerator(comp))
                {
                    NodeSingle other = comp.OtherNode(node);
                    if (other == previos)
                        continue;
                    if (other.IsReference)
                        continue;
                    other.TypeOfNode = NodeSingle.NodeType.VoltageFixedNode;
                    
                    ScanEarthSuperNode(other, node);
                }
            }
        }

        /// <summary>
        /// Nodes in a supernode are connected with voltage 
        /// generators in most analisys
        /// </summary>
        /// <param name="compo"></param>
        /// <returns>Return True if compo is Voltage Generator</returns>
        protected virtual bool IsPartOfSuperNode(Dipole compo)
        {
            return compo is VoltageGenerator || compo is Inductor;
        }

        public override bool Solve(Circuit cir, BasicAnalysis ana)
        {
            cir.State = Circuit.CircuitState.Solving;

            SolveInfo solveinfo = PreAnalizeToSolve(cir);

            //se calculas las tensiones de nodos
            #region fase 1

            Calculate(solveinfo);

            #endregion

            //Con las tensiones de nodos, se calculan las corrientes
            #region fase 2

            CalculateCurrents(CurrentCircuit, null);

            #endregion

            cir.State = Circuit.CircuitState.Solved;

            return true;    
        }

        /// <summary>
        /// Export Nodes and Currents to some file
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public virtual bool Export(string FileName, ExportFormats format = ExportFormats.CSV)
        {
            //se guarda las tensiones
            //     N1  N2 .... Nn 
            //    5   15      2             Volts


            // las corrientes
            //  Comp1   Comp2   Comp3...
            //    1       2       -1        Amperes
            //List<List<string>> todos = new List<List<string>>();
            using (var writer = new CsvFileWriter(FileName))
            {
                writer.Delimiter = ';';
                List<string> results = new List<string>();
                foreach (var node in CurrentCircuit.Nodes)
                {
                    results.Add(node.Key.ToString());
                }
                writer.WriteRow(results);
                results.Clear();

                foreach (var node in CurrentCircuit.Nodes)
                {
                    results.Add(node.Value.Voltage.ToString());
                }
                writer.WriteRow(results);
                results.Clear();

                List<string> componames = new List<string>();
                ScanComponentBlockCurrents(results, componames, CurrentCircuit);
                writer.WriteRow(componames);
                writer.WriteRow(results);
                return true;
            }
        }

        /// <summary>
        /// escanea los componentes de un contenedor y almacena los nombres 
        /// y corrientes de cada uno
        /// </summary>
        /// <param name="results"></param>
        /// <param name="componames"></param>
        /// <param name="container"></param>
        private void ScanComponentBlockCurrents(List<string> results, List<string> componames, ComponentContainer container)
        {
            foreach (var compo in container.Components)
            {
                if (compo is Block)
                    ScanComponentBlockCurrents(results, componames, (ComponentContainer)compo);
                else
                {
                    results.Add(((Dipole)compo).current.ToString());
                    componames.Add(compo.Name);
                }
            }
        }

        /// <summary>
        /// Calculate Nodes Voltages 
        /// </summary>
        /// <param name="solveinfo"></param>
        /// <param name="e"></param>
        protected virtual void Calculate(SolveInfo solveinfo, object e = null)
        {

            // Fixed Voltage Nodes
            //if (e is double)
            //    Debug.Assert((double)e < 0.0008f);

            FindFixedVoltages(solveinfo, solveinfo.Circuit.Reference, null, e);
            
            if (solveinfo.RowCount > 0)
            {
                #region matrix solving region
                int fila = 0, columna = 0;

                var v = Vector<Complex>.Build.Dense(solveinfo.RowCount);
                var A = Matrix<Complex>.Build.DenseOfArray(new Complex[solveinfo.RowCount, 
                                                        solveinfo.ColumnsCount]);

                foreach (var nodo in solveinfo.NortonNodes)
                {
                #region nodos normales
                    fila = solveinfo.RowIndexOf(nodo);
                    CalculateNodeImpedances(solveinfo, fila, A, v, nodo, e);
                #endregion
                }

                foreach (var snode in solveinfo.SuperNodes)
                {
                #region supernodos
                    #region ecuaciones de supernodos

                    fila = solveinfo.RowIndexOf(snode);
                    CalculateNodeImpedances(solveinfo, fila, A, v, snode, e);

                    #endregion

                    #region Ecuaciones de tensiones en par de nodos

                    foreach (var V in snode.Components)
                    {
                        columna = solveinfo.ColumnIndexOf(V.Nodes[0]);
                        fila = solveinfo.RowIndexOf(V);
                        v[fila] = GetVoltage(V, V.Nodes[0], e);
                        A[fila, columna] = 1;
                        columna = solveinfo.ColumnIndexOf(V.Nodes[1]);
                        A[fila, columna] = -1;
                    }

                    #endregion
                #endregion
                }

                foreach (Dipole comp in solveinfo.SpecialComponents)
                {
                #region special controlled component
                    //ecuacion del componente
                    fila = solveinfo.RowIndexOf(comp);
                    if (comp is ControlledVoltageGenerator)
                        foreach (var nodo in comp.Nodes)
                        {
                            if (nodo.IsReference)
                                continue;
                            columna = solveinfo.ColumnIndexOf(nodo);
                            A[fila, columna] = ((ControlledDipole)comp).ControllerEquationValue(nodo, e, false);
                        }
                    v[fila] = ((ControlledDipole)comp).ControllerEquationValue(0, e, false);

                    List<NodeSingle> inputnodes = new List<NodeSingle>();
                    if (comp is ControllerShortCircuit)
                        //proceso las filas: corrientes en la entrada
                        inputnodes = ((ControllerShortCircuit)comp).InputNodes;
                    else if (comp is ControllerOpenCircuit)
                        inputnodes = ((ControllerOpenCircuit)comp).InputNodes;

                    if (comp is ControlledCurrentGenerator)
                    {
                        inputnodes = ((ControllerOpenCircuit)comp).InputNodes;
                        columna = solveinfo.ColumnIndexOf(comp, null);
                        A[fila, columna] = Complex.One;
                    }
                    
                    foreach (var nodo in inputnodes)
                    {
                        if (nodo.IsReference)
                            continue;
                        columna = solveinfo.ColumnIndexOf(nodo);
                        A[fila, columna] = ((ControlledDipole)comp).ControllerEquationValue(nodo, e, true);
                    }
                #endregion
                }

                var x = A.Solve(v);

                //saving nodes voltages
                foreach (var nodo in solveinfo.Nodes)
                {
                    fila = solveinfo.ColumnIndexOf(nodo);
                    if (double.IsNaN(x[fila].Magnitude))
                        NotificationsVM.Instance.Notifications.Add(
                        new Notification("Invalid value voltage at node " + nodo.Name 
                                        + " and parameter " + e.ToString(),
                                        Notification.ErrorType.error));
                    nodo.Voltage = x[fila];
                    //Debug.Assert(double.NaN != nodo.Voltage.Real);
                    //if (double.NaN == nodo.Voltage.Real)
                }
                foreach (var nodo in solveinfo.SpecialComponentNodes)
                {
                    fila = solveinfo.ColumnIndexOf(nodo);
                    nodo.Voltage = x[fila];
                   
                }
                #endregion
            }
        }

        /// <summary>
        /// Find all voltage generator connected to earth reference
        /// and calculate then each node voltage of this earth supernode
        /// </summary>
        /// <param name="solveinfo"></param>
        /// <param name="nodo"></param>
        /// <param name="previousnode"></param>
        /// <param name="e"></param>
        protected virtual void FindFixedVoltages(SolveInfo solveinfo, NodeSingle nodo, 
                                                    NodeSingle previousnode,object e)
        {
            foreach (var comp in nodo.Components)
            {
                if (IsPartOfSuperNode(comp))
                {
                    NodeSingle other = comp.OtherNode(nodo);
                    if (other == previousnode)
                        continue;
                    other.Voltage = nodo.Voltage + GetVoltage(comp, other, e);
                    FindFixedVoltages(solveinfo, other, nodo, e);
                }
            }
        }

        protected virtual Complex GetVoltage(Dipole V, NodeSingle nodo, object e)
        {
            if (e is double)
                return V.voltage(nodo, (double)e);
            if (V.Nodes[0] == nodo)
                return V.Voltage;
            return -V.Voltage;
        }

        private SuperNode super;

        /// <summary>
        /// Calculate SUM(1/Ri) in equation given for fila and nodes 
        /// asociated with nodo
        /// </summary>
        /// <param name="solveinfo"></param>
        /// <param name="fila"></param>
        /// <param name="A"></param>
        /// <param name="nodo"></param>
        protected virtual void CalculateNodeImpedances(SolveInfo solveinfo, int fila,
                                                        Matrix<Complex> A, Vector<Complex> v,
                                                        Node nodo,
                                                        object e = null)
        {
            
            if (nodo is SuperNode)
            {
                super = nodo as SuperNode;
                foreach (var nodo1 in ((SuperNode)nodo).Nodes)
                {
                    CalculateNodeImpedances(solveinfo, fila, A, v, nodo1, e);
                }
                super = null;
            }
            else
            {
                int columna = solveinfo.ColumnIndexOf((NodeSingle)nodo);
                foreach (var comp in nodo.Components)
                {
                    //SUM(1/Ri)
                    //deberian ser todos pasivos
                    if (IsImpedance(comp))
                    {
                        columna = solveinfo.ColumnIndexOf((NodeSingle)nodo);
                        A[fila, columna] += 1 / GetImpedance(comp, e);

                        NodeSingle other = comp.OtherNode((NodeSingle)nodo);

                        if (other.IsReference)
                            continue;
                        if (solveinfo.AutoCalculableNodes.Contains(other))
                        {
                            columna = solveinfo.ValueIndexOf(other);

                            v[fila] += other.Voltage / GetImpedance(comp, e);
                        }
                        else if (solveinfo.NortonNodes.Contains(other))
                        {
                            columna = solveinfo.ColumnIndexOf(other);
                            A[fila, columna] -= 1 / GetImpedance(comp, e);

                        }
                        else if (solveinfo.NodesInSupernodes.Contains(other))
                        {
                            columna = solveinfo.ColumnIndexOf(other);
                            if (super != null && super.Nodes.Contains(other))
                                A[fila, columna] += 1 / GetImpedance(comp, e);
                            else
                                A[fila, columna] -= 1 / GetImpedance(comp, e);

                        }
                        else if (solveinfo.SpecialComponentNodes.Contains(other))
                        {
                            columna = solveinfo.ColumnIndexOf(other);
                            A[fila, columna] -= 1 / GetImpedance(comp, e);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else if (comp is ControlledCurrentGenerator)
                    {
                        columna = solveinfo.ColumnIndexOf(comp, nodo);
                        A[fila, columna] = ((ControlledDipole)comp).ControllerEquationValue(nodo, e, false);
                    }
                    else if (comp is ControlledCurrentGenerator)
                    {
                        throw new NotImplementedException();
                    }
                    else if (IsCurrentGenerator(comp))
                    {
                        //columna = solveinfo.ColumnIndexOf(nodo);
                        //NodeSingle other = comp.OtherNode(nodo as NodeSingle);
                        var i = GetCurrent(comp, nodo as NodeSingle, e);
                        v[fila] -= i;
                    }
                }
            }
        }

        protected virtual Complex GetCurrent(Dipole comp, NodeSingle nodo, object e)
        {
            if (e is Complex)
                return comp.Current(nodo, e as Complex?);
            if (e is Double)
                return comp.Current(nodo, (double)e);
            if (e == null)
                return comp.current;
            throw new NotSupportedException();
        }

        protected virtual Complex GetImpedance(Dipole comp, object e)
        {
            if (e is Complex)
                return comp.Impedance(e as Complex?);
            return comp.Impedance();
        }


        /// <summary>
        /// Calculate Statics components currents
        /// </summary>
        /// <param name="container"></param>
        protected virtual void CalculateCurrents(ComponentContainer container,
                                                object e)
        {
            foreach (var comp in container.Components)
            {
                //la corriente de DC de un capacitor es 0
                if (IsOpenCircuit(comp))
                {
                    comp.Current(0);
                    continue;
                }

                //la corriente en las resistencias se calcula directamente en ellas: es ley de Ohm:V/R
                if (IsImpedance(comp) || IsCurrentGenerator(comp))
                {
                    //NodeSingle nodo = comp.Nodes[0];
                    //Complex i = GetCurrent(comp, nodo, e);
                    //if (container is Branch)
                    //{
                    //    ((Branch)container).current = i;
                    //}
                    continue;
                }


                //en los Generadores de tension hay que calcular la corriente en 1 u ambos nodos
                if (IsVoltageGenerator(comp) || IsShortCircuit(comp))
                {
                    foreach (var nodo in comp.Nodes)
                    {
                        Dipole comp2 = nodo.OtherComponent(comp);
                        //si tiene solo un resistor en serie es automatico el valor de corriente
                        if (nodo.Components.Count == 2 && 
                            (IsImpedance(comp2) || IsCurrentGenerator(comp2)))
                        {
                            comp.Current(-GetCurrent(comp2, nodo, e), nodo);
                       
                            //if (container is Branch)
                            //    ((Branch)container).current = GetCurrent(comp, nodo, e);
                            goto out1;
                        }
                    }
                    //si no tiene solo una resistencias en serie, es decir, un nodo de multiples ramas
                    //se aplica 2da de Kirchoff para el supernodo
                    // throw new NotImplementedException();
                    foreach (var nodo in comp.Nodes)
                    {
                        if (nodo.IsReference)
                        {
                            continue;
                        }
                        Complex i = Complex.Zero;
                        i = CalculateSupernodeCurrent(nodo, e, comp);
                        comp.Current(i, nodo);
                        break;
                    }
                }
                //else if (comp is Branch)
                //{
                //    CalculateCurrents((Branch)comp, e);
                //    continue;
                //}

                //else if (comp is ParallelBlock)
                //{
                //    CalculateCurrents((ParallelBlock)comp, e);
                //    continue;
                //}

                out1:;
            }
        }

        protected virtual bool IsCurrentGenerator(Dipole comp)
        {
            return comp is CurrentGenerator;
        }

        protected virtual bool IsImpedance(Dipole comp)
        {
            return comp is Resistor;
        }

        protected virtual bool IsShortCircuit(Dipole comp)
        {
            return comp is Inductor;
        }

        protected virtual bool IsVoltageGenerator(Dipole comp)
        {
            return (comp is VoltageGenerator || comp is Inductor) && !(comp is ControlledDipole);
        }

        protected virtual bool IsOpenCircuit(Dipole comp)
        {
            return comp is Capacitor;
        }


        /// <summary>
        /// Dado un supernodo, recorre todas sus ramas para hallar la 
        /// corriente en una de ellas
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="W"></param>
        /// <returns></returns>
        protected virtual Complex CalculateSupernodeCurrent(NodeSingle nodo, 
                                                            object e, 
                                                            Dipole comp)
        {
            Complex i = Complex.Zero;
            foreach (var comp2 in nodo.Components)
            {
                if (comp2 == comp)
                {
                    continue;
                }
                if (IsVoltageGenerator(comp2))
                {
                    NodeSingle nodo1 = comp2.OtherNode(nodo);
                    i += CalculateSupernodeCurrent(nodo1, e, comp2);
                }
                else
                    i += GetCurrent(comp, nodo, e);
            }
            return i;
        }


    }
}
