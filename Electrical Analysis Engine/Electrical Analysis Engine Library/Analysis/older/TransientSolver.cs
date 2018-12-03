using ElectricalAnalysis.Components;
using ElectricalAnalysis.Components.Controlled;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class TransientSolver: CircuitSolver
    {
        Circuit circuit;
        SolveInfo solveinfo;


        //                  time             nodes voltages
        public Dictionary<double, Dictionary<string, double>> Voltages { get; protected set; }
        public Dictionary<double, Dictionary<string, double>> Currents { get; protected set; }

        public bool Optimize(Circuit cir)
        {
            List<Dipole> yaanalizados = new List<Dipole>();
            List<ParallelBlock> paralelos = new List<ParallelBlock>();
            List<Node> nodosanalizados = new List<Node>();
            ParallelBlock para = null;
            solveinfo = new SolveInfo();

            #region Chequeo de nodos sueltos o componentes colgantes
            foreach (var nodo in cir.Nodes.Values)
            {
                if (nodo.Components.Count == 0)
                {
                    throw new NotImplementedException();
                }
                //componente con borne suelto
                //if (nodo.Components.Count == 1)
                //{
                //    throw new NotImplementedException();
                //}
            }
            #endregion

            #region busco componentes en paralelo

            if (false)
            //no se hace un paralelo si solo hay 2 componentes
            if (cir.Components.Count > 2)
            { 
                for (int i = 0; i < cir.Components.Count - 1; i++)
                {
                    if (yaanalizados.Contains(cir.Components[i]))
                        continue;
                    Dipole comp1 = cir.Components[i];

                    for (int j = i + 1; j < cir.Components.Count; j++)
                    {
                        if (yaanalizados.Contains(cir.Components[j]))
                            continue;
                        Dipole comp2 = cir.Components[j];

                        //comparten 2 nodos
                        if ((comp1.Nodes[0] == comp2.Nodes[0] && comp1.Nodes[1] == comp2.Nodes[1]) ||
                            (comp1.Nodes[0] == comp2.Nodes[1] && comp1.Nodes[1] == comp2.Nodes[0]))
                        {
                            if (para == null)
                            {
                                para = new ParallelBlock(cir, comp1, comp2);
                                paralelos.Add(para);
                                yaanalizados.Add(comp1);
                                yaanalizados.Add(comp2);
                            }
                            else
                            {
                                para.Components.Add(comp2);
                                yaanalizados.Add(comp2);
                            }
                        }
                    }
                }
            }
           

            #endregion

            #region reemplazo los componentes paralelos separados por
            //el correspondiente block paralelo
            foreach (var para1 in paralelos)
            {
                foreach (var compo in para1.Components)
                    cir.Components.Remove(compo);

                cir.Components.Add(para1);
            }
            #endregion

            #region identifico la tierra

            Node tierra = cir.Reference;
            if (tierra == null)
                foreach (var item in cir.Nodes.Values)
                {
                    if (item.IsReference)
                    {
                        tierra = item;
                        cir.Reference = tierra;
                        break;
                    }
                }

            #endregion

            //guardo para futuro los nodos de los componentes especiales
            if (cir.OriginalCircuit != null)
            {
                foreach (var comp in cir.OriginalCircuit.Components)
                {
                    if (comp is ControlledDipole)
                    {
                        foreach (var nodo in comp.Nodes)
                        {
                            if (!nodo.IsReference)
                            {
                                SpecialComponentInfo info = new SpecialComponentInfo(comp);
                                info.OutputNodes.Add(nodo);
                                solveinfo.specialcomponents.Add(info);
                                if (!solveinfo.nortonnodes.Contains(nodo))
                                    solveinfo.nortonnodes.Add(nodo);
                                //especialcomponents.Add(nodo, comp);
                            }
                        }
                    }
                }
            }


            #region arranco desde tierra he identifico los nodos de tension fija,

            List<Dipole> componenttoearh = new List<Dipole>();
            componenttoearh.AddRange(tierra.Components);
            SuperNodeH1 super = null;
            foreach (var compo in componenttoearh)
            {
                //los nodos de tension fija 
                if (compo is VoltageGenerator || compo is Capacitor)
                { 
                    Node nodo = compo.OtherNode(tierra);
                    nodo.TypeOfNode = Node.NodeType.VoltageFixedNode;
                    //other.Voltage = compo.Voltage;
                    solveinfo.calculablenodes.Add(nodo);
                    foreach (var comp1 in nodo.Components)
                    {
                        if (comp1 == compo)
                            continue;

                        if (comp1 is VoltageGenerator || comp1 is Capacitor)
                        {
                            if (super == null)
                                super = new SuperNodeH1();
                            super.MainNode = nodo;
                            super.Add(nodo);
                            SuperNodeH1.FindSuperNodeElements(nodo, comp1, super, nodosanalizados);
                        }
                    }
                    if (super != null)
                        solveinfo.SuperNodes.Add(super);
                    super = null;
                }

            }
            
            #endregion

            #region posibles nodos flotantes de corriente y ramas conectadas a tierra

            foreach (var compo in componenttoearh)
            {

                foreach (var rama in solveinfo.ramas)
                {
                    if (rama.Components.Contains(compo))
                        goto end;
                }      
                //desde tierra los nodos son:
                //a: pertenecen a una rama. hay que levantar dicha rama...
                //b: son nodos normales flotantes (se aplica teorema de nodo)
                Branch br = FindBranch(cir, tierra, compo);
                //si no forma parte de una rama, la rama de 1 elemento se desecha
                ValidateBranch(yaanalizados, solveinfo, tierra, compo, br);
        end: ;
            }
            #endregion

            #region Analizo las ramas faltantes desde los nodos normales encontrados

            foreach (var nodo in solveinfo.nortonnodes)
            {
                foreach (var compo in nodo.Components)
                {
                    if (solveinfo.ramas.Contains(compo) || yaanalizados.Contains(compo))
                        continue;

                    Branch br = FindBranch(cir, nodo, compo);
                    ValidateBranch(yaanalizados, solveinfo, nodo, compo, br);
                }
            }

            #endregion

        

            //reemplazo los componentes que arman una rama por la propia rama
            foreach (var rama in solveinfo.ramas)
                foreach (var compo in rama.Components)
                    cir.Components.Remove(compo);


            cir.Components.AddRange(solveinfo.ramas);
            cir.State = Circuit.CircuitState.Optimized;

            return true;
        }


        protected static Branch FindBranch(Circuit cir, Node initialNode, Dipole Component)
        {
            Branch br = new Branch(cir);
            Dipole compo = Component;
            Node nodo = Component.OtherNode(initialNode);

            br.Nodes.Clear();
            //solo es valido si el circuito fue optimizado, y los paralelos
            //se reemplazaron por bloques paralelo
            while (nodo.Components.Count == 2 && !nodo.IsReference)
            {
                br.Components.Add(compo);
                br.InternalNodes.Add(nodo);
                compo = nodo.OtherComponent(compo);
                nodo = compo.OtherNode(nodo);
            }
            br.Components.Add(compo);
            if (br.Components.Count > 1)
            {
                initialNode.Components.Remove(Component);
                initialNode.Components.Add(br);
                nodo.Components.Remove(compo);
                nodo.Components.Add(br);
               // if (!nodo.IsReference)
                nodo.TypeOfNode = Node.NodeType.MultibranchCurrentNode;
            }
            br.Nodes.Add(initialNode);
            br.Nodes.Add(nodo);

            //hay que recorrer la rama para buscar los nodos intermedios flotantes 
            //y aquellos dependientes de los flotantes
            nodo = compo.OtherNode(nodo);
            while (nodo != initialNode)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageFixedNode)
                {
                    // nothing, node was calculated
                }
                else if (compo is VoltageGenerator || compo is Capacitor)
                {
                    if (compo.IsConnectedToEarth)
                    {
                        nodo.TypeOfNode = Node.NodeType.VoltageFixedNode;
                    }
                    else if (nodo.TypeOfNode == Node.NodeType.Unknow)
                        nodo.TypeOfNode = Node.NodeType.VoltageLinkedNode;
                    else
                    { }// throw new NotImplementedException();
                }
                else if (compo is Resistor)
                {
                    if (nodo.TypeOfNode == Node.NodeType.Unknow)
                        nodo.TypeOfNode = Node.NodeType.VoltageDivideNode;
                    else
                        throw new NotImplementedException();
                }
                else if (compo is Inductor || compo is CurrentGenerator)
                { 
                    //nada, solo imponen la corriente
                }
                else
                {
                    if (nodo.TypeOfNode == Node.NodeType.Unknow)
                        nodo.TypeOfNode = Node.NodeType.InternalBranchNode;
                    else
                        throw new NotImplementedException();
                }

                compo = nodo.OtherComponent(compo);
                nodo = compo.OtherNode(nodo);
            }


            return br;
        }

        /// <summary>
        /// Valida una rama y la agrega al circuito si corresponde
        /// En caso contrario solo agrega el componente indicado
        /// </summary>
        /// <param name="yaanalizados"></param>
        /// <param name="nodosnormales"></param>
        /// <param name="ramas"></param>
        /// <param name="nodo"></param>
        /// <param name="compo"></param>
        /// <param name="br"></param>
        protected static void ValidateBranch(List<Dipole> yaanalizados, SolveInfo solveinfo, 
                                                Node nodo, Dipole compo, Branch br)
        {
            if (br.Components.Count <= 1)
            {
                if (compo is PasiveComponent)
                {
                    Node other = compo.OtherNode(nodo);
                    if (!solveinfo.nortonnodes.Contains(other) && !other.IsReference)
                    {
                        other.TypeOfNode = Node.NodeType.MultibranchCurrentNode;
                        solveinfo.nortonnodes.Add(other);
                    }
                }
                yaanalizados.Add(compo);
            }
            else
            {
                //rama valida
                solveinfo.ramas.Add(br);
                Node other = br.OtherNode(nodo);
                if (!solveinfo.nortonnodes.Contains(other) && !other.IsReference)
                    solveinfo.nortonnodes.Add(other);
                yaanalizados.AddRange(br.Components);
                foreach (var comp in br.Components)
                {
                    comp.Owner = br;
                }
            }
        }

        public bool Solve(Components.Circuit cir, BasicAnalysis ana)
        {
            List<Node> nodos = new List<Node>();
            Voltages = new Dictionary<double, Dictionary<string, double>>();
            Currents = new Dictionary<double, Dictionary<string, double>>();
            circuit = cir;
            solveinfo.calculatednodes.Clear();
            nodos.AddRange(cir.Nodes.Values);
            nodos.Remove(cir.Reference);

            //PreAnalizeToSolve(cir, nodos, solveinfo);
       
            TransientAnalysis analis = ana as TransientAnalysis;
            double t, tf, deltat;

            deltat = StringUtils.DecodeString(analis.Step);
            tf = StringUtils.DecodeString(analis.FinalTime);
            t = 0;
            cir.CircuitTime = t;
            cir.Reset();

            while (t < tf)
            {
                //Calculo de tensiones de nodos
                Calculate(solveinfo, t);

                Dictionary<string, double> result = new Dictionary<string, double>();

                #region almacenamiento temporal

                foreach (var nodo in nodos)
                {
                    result.Add(nodo.Name, nodo.Voltage.Real);
                }

                if (!Voltages.ContainsKey(t))
                    Voltages.Add(t, result);
                #endregion

                //calculo las corrientes:
                CalculateCurrents(cir, t);
                Dictionary<string, double> currents = new Dictionary<string, double>();
                StorageCurrents(cir, currents);
                Currents.Add(t, currents);


                cir.CircuitTime = t;
                t += deltat;
            }
            cir.State = Circuit.CircuitState.Solved;

            return true;
        }

        /// <summary>
        /// Calculate voltages in each node of circuit at given time
        /// </summary>
        /// <param name="solveinfo">information needed to calculate voltages </param>
        /// <param name="t">actual time to calculate</param>
        protected static void Calculate(SolveInfo solveinfo, double t)
        {

            if (solveinfo.nortonnodes.Count == 0 && solveinfo.calculablenodes.Count == 0)
                return;

            //tensiones que se calculan directamente
            #region tensiones fijas y Supernodos

            foreach (var nodo in solveinfo.calculablenodes)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageFixedNode)
                {
                    foreach (var compo in nodo.Components)
                    {
                        if (compo.IsConnectedToEarth && (compo is VoltageGenerator || compo is Capacitor))
                        {
                            nodo.Voltage = new Complex(compo.voltage(nodo, t), 0);   //el componente conectado a tierra debe ser Vdc o Vsin o capacitor
                            solveinfo.calculatednodes.Add(nodo);

                            foreach (var super in solveinfo.SuperNodes)
                            {
                                if (super.MainNode == nodo)
                                {
                                    super.CalculateLinkedVoltages(solveinfo, t);
                                    break;
                                }
                            }

                            break;
                        }
                    }
                    continue;
                }
            }

            #endregion

            //ESTO HAY QUE ARREGLARLO
            #region Tensiones de nodos que se calculan mediante matriz

            if (solveinfo.nortonnodes.Count > 0)
            {
                int fila = 0, columna = 0;
                var Currs = Vector<double>.Build.Dense(solveinfo.MatrixDimension);
                var A = Matrix<double>.Build.DenseOfArray(new double[solveinfo.MatrixDimension, solveinfo.MatrixDimension]);
                List<Node> solvednodes = new List<Node>();

                foreach (var nodo in solveinfo.nortonnodes)
                {
                    if (solvednodes.Contains(nodo))
                    {
                        //no deberia pasar
                        continue;
                    }
                    columna = fila = solveinfo.nortonnodes.IndexOf(nodo);
                    solvednodes.Add(nodo);
                    //if (nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode)
                    //{
                    //    foreach (var comp in nodo.Components)
                    //    {
                    //        if (comp is VoltageGenerator || comp is Capacitor)
                    //            Currs[fila] = comp.voltage(nodo, t);

                    //        A[fila, columna] = 1;
                    //    }
                        
                    //}
                    //else
                    if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                            nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                    {
                        double Z = 0, I = 0, Y = 0, V = 0;

                        foreach (var comp in nodo.Components)
                        {
                            SolveNortonNode(nodo, comp, solvednodes, ref Z, ref I, ref Y, ref V, t);
                        }
                        Currs[fila] = I + V * Y / 2;
                        A[fila, columna] = Y;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                var x = A.Solve(Currs);

                //asigno las tensiones calculadas
                foreach (var nodo in solveinfo.nortonnodes)
                {
                    fila = solveinfo.nortonnodes.IndexOf(nodo);
                    if (!double.IsNaN(x[fila]))
                        nodo.Voltage = new Complex(x[fila], 0);
                }
            }
            #endregion

            #region divisor de tension

            //existen nodos donde la tension se puede calcular casi directamente
            foreach (var nodo in solveinfo.calculablenodes)
            {
                if (solveinfo.calculatednodes.Contains(nodo))
                    continue;

                if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                {
                    Node nodo1 = nodo;
                    Dipole compo1 = nodo.Components[0];
                    Branch br = compo1.Owner as Branch;
                    if (br == null)
                    {
                        throw new Exception();
                    }
                    double v1, v2, z1, z2;
                    v1 = v2 = z1 = z2 = 0;
                    //el nodo es interno en una rama, deberian los 2 componentes conectados
                    //al nodo, 
                    Dipole compo2 = nodo.Components[1];
                    //hacia la izq (o derec)
                    NavigateBranch(nodo1, compo1, t, ref v1, ref z1);
                    //hacia el otro lado
                    NavigateBranch(nodo1, compo2, t, ref v2, ref z2);
                    nodo.Voltage = new Complex( ((z2 * v1 + v2 * z1) / (z1 + z2)),0);
                }
                else if (nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode)
                {
                
                
                }
                //else 

            }

            #endregion

            #region analisis de ramas

            //parto del extremo de la rama
            foreach (var rama in solveinfo.ramas)
            {
                Node nodo1 = null;
                //busco nodos ya calculados en la rama
                foreach (var nodo in rama.InternalNodes)
                {
                    if (!solveinfo.calculablenodes.Contains(nodo) && !solveinfo.nortonnodes.Contains(nodo))
                    {
                        nodo1 = nodo;
                        break;
                    }
                }
                if (nodo1 == null || solveinfo.calculatednodes.Contains(nodo1))
                    continue;

                double i = 0;
                //encuentro la corriente impuesta de la rama si es que la hay
                //nodo1 = rama.Nodes[0];
                
                i = rama.Current(nodo1, t);
                Dipole compo1 = null;
                foreach (var compo in rama.Components)
	            {
                    //busco el componente ubicado en un extremo de la rama
                    if (compo.Nodes[0] == nodo1 || compo.Nodes[1] == nodo1)
                    {
                        compo1 = compo;
                        break;
                    }
	            }
                Node nodo2 = compo1.OtherNode(nodo1);
                if (compo1 is VoltageGenerator)
                {
                    //nodo de tension vinculada
                    nodo1.Voltage = nodo2.Voltage + new Complex( compo1.voltage(nodo2, t), 0);
                }
                else if (compo1 is Resistor)
                {
                    //nodo interno, pero conocemos la corriente
                    nodo1.Voltage = nodo2.Voltage - new Complex( i * compo1.Impedance().Real, 0);
                }
                else
                {
                    throw new NotImplementedException();
                }
                //if (nodo.TypeOfNode == Node.NodeType.InternalBranchNode)
                //{
                //}
            }
            #endregion
        }

      

        private static void SolveNortonNode(Node nodo, Dipole comp, List<Node> solvednodes, ref double Z, ref double I, ref double Y, ref double V, double t)
        {
            if (comp is Branch)
            {
                I += ((Branch)comp).NortonCurrent(nodo, t);
                Z = comp.Impedance().Real;
                Y += 1 / Z;
            }
            else if (comp is Resistor)
            {
                Z = comp.Impedance().Real;
                Y += 1 / Z;
            }
            else if (comp is CurrentGenerator || comp is Inductor)
            {
                I += comp.Current(nodo, t);
            }
            else if (comp is VoltageGenerator || comp is Capacitor)
            {
                V += comp.voltage(nodo, t);
                Node nodo2 = comp.OtherNode(nodo);
                if (nodo2.IsReference || solvednodes.Contains(nodo2))
                    return;

                solvednodes.Add(nodo2);
                SolveNortonNode(nodo2, comp, solvednodes, ref Z, ref I, ref Y, ref V, t);
               
            }
        }

        /// <summary>
        /// barre una rama desde un nodo interno hacia la izquierda o derecha
        /// calculando la suma de las tensiones de generadores V1
        /// y la suma de impedancias Z1
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="compo1"></param>
        /// <param name="t"></param>
        /// <param name="v1"></param>
        /// <param name="z1"></param>
        private static void NavigateBranch(Node nodo, Dipole compo1, double t, ref double v1, ref double z1)
        {
            Node nodo1 = nodo;

            while (!(nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode || nodo1.IsReference))
            {
                if (compo1 is Resistor)
                {
                    z1 += compo1.Impedance().Real;
                }
                else if (compo1 is VoltageGenerator || compo1 is Capacitor)
                {
                    v1 += compo1.voltage(nodo1, t);
                }
                else
                {
                    throw new NotImplementedException();
                }
                nodo1 = compo1.OtherNode(nodo1);
                compo1 = nodo1.OtherComponent(compo1);
            }
            if (nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode)
            {
                v1 += nodo1.Voltage.Real;
            }
        }


        private static void StorageCurrents(ComponentContainer cir, Dictionary<string, double> currents)
        {
            foreach (var compo in cir.Components)
            {
                currents.Add(compo.Name, compo.current.Real);
                if (compo is ComponentContainer)
                {
                    StorageCurrents((ComponentContainer)compo, currents);
                }
            }
        }

        protected static void CalculateCurrents(ComponentContainer container, double t)
        {

            foreach (var comp in container.Components)
            {
                //la corriente en las resistencias se calcula directamente en ellas: es ley de Ohm:V/R
                if (comp is CurrentGenerator || comp is Inductor || comp is Resistor)
                {
                    Node nodo = comp.Nodes[0];
                    //comp.current = new Complex(comp.Current(nodo, t), 0);
                    if (container is Branch)
                    {
                        ((Branch)container).current = new Complex(comp.Current(nodo, t), 0);
                    }
                    continue;
                }


                //en los Generadores de tension hay que calcular la corriente en 1 u ambos nodos
                if (comp is VoltageGenerator || comp is Capacitor)
                {
                    foreach (var nodo in comp.Nodes)
                    {
                        if (nodo.IsReference)
                            continue;
                        Dipole comp2 = nodo.OtherComponent(comp);
                        //si tiene solo un resistor en serie es automatico el valor de corriente
                        if (nodo.Components.Count == 2 && (comp2 is Resistor || comp2 is CurrentGenerator || comp2 is Inductor))
                        {
                            if (nodo == comp.Nodes[0])
                                comp.current = new Complex(comp2.Current(nodo, t), 0);
                            else
                                comp.current = new Complex(-comp2.Current(nodo, t), 0);
                            if (container is Branch)
                                ((Branch)container).current = comp.current;
                            goto out1;
                        }
                    }
                    Node node = comp.Nodes[0];
                    //si no tiene solo una resistencias en serie, es decir, un nodo de multiples ramas
                    //se aplica 2da de Kirchoff para el supernodo
                    comp.current = new Complex(CalculateSupernodeCurrent(node, comp, t), 0);
                   
                }
                else if (comp is Branch)
                {
                    CalculateCurrents((Branch)comp, t);
                    continue;
                }

                else if (comp is ParallelBlock)
                {
                    CalculateCurrents((ParallelBlock)comp, t);
                    continue;
                }

            out1: ;
            }
        }

        /// <summary>
        /// Dado un supernodo, recorre todas sus ramas para hallar la corriente en una de ellas
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="W"></param>
        /// <returns></returns>
        private static double CalculateSupernodeCurrent(Node nodo, Dipole comp, double t)
        {
            if (nodo.IsReference)
                nodo = comp.OtherNode(nodo);

            double i = 0;
            foreach (var comp2 in nodo.Components)
            {
                if (comp2 == comp)
                {
                    continue;
                }
                if (comp2 is VoltageGenerator)
                {
                    Node nodo1 = comp2.OtherNode(nodo);
                    i += CalculateSupernodeCurrent(nodo1, comp2, t);
                }
                else
                    i += comp2.Current(nodo, t);
            }
            return i;
        }

        public void ExportToCSV(string FileName)
        {

            //se guarda@)
            //t     N1  N2 .... Nn      cmp1    cmp2    ....
            //10    5   15      2       1mA     2mA
            //12    6   14      1       0,5mA   3mA
            //List<List<string>> todos = new List<List<string>>();
            using (var writer = new CsvFileWriter(FileName))
            {
                writer.Delimiter = ';';
                List<string> results = new List<string>();
                results.Add("t");
                //List<string> componames = new List<string>();
                //results.Clear();
                foreach (var item in Voltages)
                {
                    foreach (var node in item.Value)
                        results.Add(node.Key.ToString());
                    break;
                }
                foreach (var compo in Currents.Values)
                {
                    foreach (var componame in compo.Keys)
                        results.Add(componame);
                    break;
                }
                writer.WriteRow(results);
               // results.Clear();

                
                foreach (var item in Voltages)
                {
                    results.Clear();
                    // tiempo
                    results.Add(item.Key.ToString());
                    //los nodos
                    foreach (var node in item.Value)
                    {
                        results.Add(node.Value.ToString());
                    }
                    //corrientes
                    Dictionary<string, double> currs = Currents[item.Key];
                    foreach (var corr in currs.Values)
                    {
                        results.Add(corr.ToString());
                    }
                    writer.WriteRow(results);
                }

                //writer.WriteRow(results);



            }

        }



        /// <summary>
        /// escanea los componentes de un contenedor y almacena los nombres y corrientes de cada uno
        /// </summary>
        /// <param name="results"></param>
        /// <param name="componames"></param>
        /// <param name="container"></param>
        [Obsolete]
        private void ScanComponentBlockCurrents(List<string> results, List<string> componames, ComponentContainer container)
        {
            foreach (var compo in container.Components)
            {
                if (compo is Block)
                    ScanComponentBlockCurrents(results, componames, (ComponentContainer)compo);
                else
                {
                    results.Add(compo.current.Real.ToString());
                    componames.Add(compo.Name);
                }
            }
        }

    }
}
