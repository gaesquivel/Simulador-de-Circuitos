using ElectricalAnalysis.Components;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Analysis.Solver
{
    public class DCSolver: CircuitSolver
    {
        Circuit circuit;


        protected static Branch FindBranch(Circuit cir, Node initialNode, Dipole Component)
        {
            Branch br = new Branch(cir);
            Dipole compo = Component;
            Node nodo = Component.OtherNode(initialNode);
            
            br.Nodes.Clear();
            //solo es valido si el circuito fue optimizado, y los paralelos
            //se reemplazaron por bloques paralelo
            while (nodo.Components.Count == 2 && cir.Reference != nodo)
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
                else if (compo is VoltageGenerator)
                {
                    if (nodo.TypeOfNode == Node.NodeType.Unknow)
                    {
                        nodo.TypeOfNode = Node.NodeType.VoltageLinkedNode;
                    }
                    else
                        throw new NotImplementedException();
                }
                else if (compo is PasiveComponent)
                { 
                    if (nodo.TypeOfNode == Node.NodeType.Unknow)
                        nodo.TypeOfNode = Node.NodeType.VoltageDivideNode;
                    else
                        throw new NotImplementedException();

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
        /// optimize a recent loaded circuit leaving ready for simulation
        /// </summary>
        /// <param name="cir"></param>
        /// <returns></returns>
        public static bool Optimize(Circuit cir)
        {
            List<Dipole> yaanalizados = new List<Dipole>();
            List<ParallelBlock> paralelos = new List<ParallelBlock>();
            List<Node> nodosnormales = new List<Node>();
            List<Branch> ramas = new List<Branch>();
            ParallelBlock para = null;

            #region Chequeo de nodos sueltos o componentes colgantes
            foreach (var nodo in cir.Nodes.Values)
            {
                if (nodo.Components.Count == 0)
                {
                    throw new NotImplementedException();
                }
                //componente con borne suelto
                if (nodo.Components.Count == 1)
                {
                    throw new NotImplementedException();
                }
            }
            #endregion

            #region busco componentes en paralelo

            if (false)
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

            #region arranco desde tierra he identifico los nodos de tension fija, 
            //posibles nodos flotantes de corriente y ramas conectadas a tierra

            List<Dipole> componenttoearh = new List<Dipole>();
            componenttoearh.AddRange(tierra.Components);
            foreach (var compo in componenttoearh)
            {
                foreach (var rama in ramas)
	            {
                    if (rama.Components.Contains(compo))
                        goto end;
		 
	            }
                //los nodos de tension fija 
                if (compo is VoltageGenerator)
                {
                    Node other = compo.OtherNode(tierra);
                    other.TypeOfNode = Node.NodeType.VoltageFixedNode;
                    other.Voltage = compo.Voltage;
                }
                //desde tierra los nodos son:
                //a: pertenecen a una rama. hay que levantar dicha rama...
                //b: son nodos normales flotantes (se aplica teorema de nodo)
                Branch br = FindBranch(cir, tierra, compo);
                //si no forma parte de una rama, la rama de 1 elemento se desecha
                ValidateBranch(yaanalizados, nodosnormales, ramas, tierra, compo, br);
           }
        end: ;
            #endregion

            #region Analizo las ramas faltantes desde los nodos normales encontrados

            foreach (var nodo in nodosnormales)
            {
                foreach (var compo in nodo.Components)
                {
                    if (ramas.Contains(compo) || yaanalizados.Contains(compo))
                        continue;

                    Branch br = FindBranch(cir, nodo, compo);
                    ValidateBranch(yaanalizados, nodosnormales, ramas, nodo, compo, br);
                }
            }

            #endregion

            //reemplazo los componentes que arman una rama por la propia rama
            foreach (var rama in ramas)
                foreach (var compo in rama.Components)
                    cir.Components.Remove(compo);
            

            cir.Components.AddRange(ramas);

            //foreach (var rama in ramas)
            //{
            //    AddComponentNodes(cir, rama);
            //}
            cir.State = Circuit.CircuitState.Optimized;
            cir.OptimizedCircuit = cir;
            return true;
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
        protected static void ValidateBranch(List<Dipole> yaanalizados, List<Node> nodosnormales, 
                                            List<Branch> ramas, Node nodo, Dipole compo, Branch br)
        {
            if (br.Components.Count <= 1)
            {
                if (compo is PasiveComponent)
                {
                    Node other = compo.OtherNode(nodo);
                    other.TypeOfNode = Node.NodeType.MultibranchCurrentNode;
                    if (!nodosnormales.Contains(other) && !other.IsReference)
                        nodosnormales.Add(other);
                }
                yaanalizados.Add(compo);
            }
            else
            {
                //rama valida
                ramas.Add(br);
                Node other = br.OtherNode(nodo);
                if (!nodosnormales.Contains(other) && !other.IsReference)
                    nodosnormales.Add(other);
                yaanalizados.AddRange(br.Components);
                foreach (var comp in br.Components)
                {
                    comp.Owner = br;
                }
            }
        }

        protected static void AddComponentNodes(Circuit ciroptimizado, Dipole rama)
        {
            if (!ciroptimizado.Nodes.ContainsValue(rama.Nodes[0]))
                ciroptimizado.Nodes.Add(rama.Nodes[0].Name, rama.Nodes[0]);
            if (!ciroptimizado.Nodes.ContainsValue(rama.Nodes[1]))
                ciroptimizado.Nodes.Add(rama.Nodes[1].Name, rama.Nodes[1]);
        }


        public virtual bool Solve(Circuit cir, BasicAnalysis ana)
        {
            //int fila = 0;
            List<Node> nodos = new List<Node>();
            circuit = cir;

            if (cir.Reference == null)
            {
                foreach (var nodo in cir.Nodes.Values)
                {
                    //creo una lista de nodos sin el nodo referencia
                    if (!nodo.IsReference)
                        nodos.Add(nodo);
                } 
            }
            else
            {
                nodos.AddRange(cir.Nodes.Values);
                nodos.Remove(cir.Reference);
            }
           

            List<Node> nodosnorton = new List<Node>();
            List<Node> nortoncopia = new List<Node>();

            foreach (var nodo in nodos)
            {
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                    nodosnorton.Add(nodo);
            }

            #region Calculo de tensiones de nodos

            nortoncopia.Clear();
            foreach (var item in nodosnorton)
            {
                nortoncopia.Add(item);
            }

            Calculate(nortoncopia);
            #endregion

            //#region almacenamiento temporal


            //#endregion
            
            //calculo las corrientes:
            CalculateCurrents(cir);


            cir.State = Circuit.CircuitState.Solved;
            return true;
        }

        protected static void Calculate(List<Node> nodosnorton)
        {
           
            if (nodosnorton.Count == 0)
                return;

            //existen nodos donde la tension se puede calcular directamente
            List<Node> nodosCalculables = new List<Node>();

            foreach (var nodo in nodosnorton)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                    nodosCalculables.Add(nodo);
            }
            foreach (var nodo in nodosCalculables)
                nodosnorton.Remove(nodo);

            //remuevo esos nodos de los que deben calcularse matricialmente

            if (nodosnorton.Count > 0)
            {
                int fila = 0, columna = 0;
                var v = Vector<Complex32>.Build.Dense(nodosnorton.Count);
                var A = Matrix<Complex32>.Build.DenseOfArray(new Complex32[nodosnorton.Count, nodosnorton.Count]);

                foreach (var nodo in nodosnorton)
                {
                    columna = fila = nodosnorton.IndexOf(nodo);
                    Complex32 Z, V;
                    if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                        nodo.TypeOfNode == Node.NodeType.InternalBranchNode)
                    {
                        foreach (var rama in nodo.Components)
                        {
                            if (rama is Branch || rama is PasiveComponent)
                            {
                                columna = fila = nodosnorton.IndexOf(nodo);
                                if (rama is Branch)
                                    V = ((Branch)rama).NortonCurrent(nodo);
                                else
                                    V = rama.Current(nodo);
                                v[fila] += V;
                                Z = rama.Impedance();
                                A[fila, columna] += 1/Z;
                                Node nodo2 = rama.OtherNode(nodo);
                                if (!nodo2.IsReference)
                                {
                                    columna = nodosnorton.IndexOf(nodo2);
                                    A[fila, columna] -= 1 / Z;
                                    
                                }
                            }
                            else  if (rama is CurrentGenerator)
                            {
                                V = rama.Current(nodo);
                                v[fila] += V;
                            }
                        }
                    }
                    else if (nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode)
                    {
                        Dipole compo = nodo.Components[0];
                        if (!(compo is VoltageGenerator))
                            compo = nodo.Components[1];
                        v[fila] = compo.Voltage.Real;
                        A[fila, columna] = 1;
                        columna = nodosnorton.IndexOf(compo.OtherNode(nodo));
                        A[fila, columna] = -1;
                    }
                    else
                        throw new NotImplementedException();
                }
                var x = A.Solve(v);

                foreach (var nodo in nodosnorton)
                {
                    fila = nodosnorton.IndexOf(nodo);
                    nodo.Voltage = x[fila];
                }
            }

            foreach (var nodo in nodosCalculables)
            {
                if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                {
                    Node nodo1 = nodo;
                    Dipole compo1 = nodo.Components[0];
                    Branch br = compo1.Owner as Branch;
                    if (br == null)
                    {
                        throw new Exception();
                    }
                    Complex32 v1, v2, z1, z2;
                    v1 = v2 = z1 = z2 = Complex32.Zero;
                    //el nodo es interno en una rama, deberian los 2 componentes conectados
                    //al nodo, 
                    Dipole compo2 = nodo.Components[1];
                    //hacia la izq (o derec)
                    NavigateBranch(nodo1, compo1, ref v1, ref z1);
                    //hacia el otro lado
                    NavigateBranch(nodo1, compo2, ref v2, ref z2);
                    nodo.Voltage = (z2 * v1 + v2 * z1) / (z1 + z2);
                    //nodosCalculables.Add(nodo);
                }
            }

            //return x;
        }

        /// <summary>
        /// barre una rama desde un nodo interno hacia la izquierda o derecha
        /// calculando la suma de las tensiones de generadores V1
        /// y la suma de impedancias Z1
        /// </summary>
        /// <param name="nodo"></param>
        /// <param name="compo1"></param>
        /// <param name="v1"></param>
        /// <param name="z1"></param>
        private static void NavigateBranch(Node nodo, Dipole compo1, ref Complex32 v1, ref Complex32 z1)
        {
            Node nodo1 = nodo;

            while (!(nodo1.TypeOfNode == Node.NodeType.MultibranchCurrentNode || nodo1.IsReference))
            {
                if (compo1 is PasiveComponent)
                {
                    z1 += compo1.Impedance();
                }
                else if (compo1 is VoltageGenerator)
                {
                    v1 += compo1.voltage(nodo1, null);
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
                v1 += nodo1.Voltage;
            }
        }


        /// <summary>
        /// Calculate Statics components currents
        /// </summary>
        /// <param name="container"></param>
        private static void CalculateCurrents(ComponentContainer container)
        {

            foreach (var comp in container.Components)
            {
                //la corriente de DC de un capacitor es 0
                if (comp is Capacitor)
                    continue;

                //la corriente en las resistencias se calcula directamente en ellas: es ley de Ohm:V/R
                if (comp is Resistor || comp is CurrentGenerator)
                {
                    if (container is Branch)
                    {
                        Node nodo = comp.Nodes[0];
                        ((Branch)container).current = comp.Current(nodo);
                    }
                    continue;
                }
               

                //en los Generadores de tension hay que calcular la corriente en 1 u ambos nodos
                if (comp is VoltageGenerator || comp is Inductor)
                {
                    foreach (var nodo in comp.Nodes)
                    {
                        Dipole comp2 = nodo.OtherComponent(comp);
                        //si tiene solo un resistor en serie es automatico el valor de corriente
                        if (nodo.Components.Count == 2 && (comp2 is Resistor || comp2 is CurrentGenerator))
                        {
                            //Node nodo2 = com
                            comp.current = comp2.Current(nodo);
                        
                            if (container is Branch)
                                ((Branch)container).current = comp.Current(nodo);
                            goto out1;
                        }
                    }
                    //si no tiene solo una resistencias en serie, es decir, un nodo de multiples ramas
                    //se aplica 2da de Kirchoff para el supernodo
                    throw new NotImplementedException();
                    foreach (var nodo in comp.Nodes)
                    {
                        Complex32 i;
                        foreach (var comp2 in nodo.Components)
                        {

                        }
                    }
                }
                else if (comp is Branch)
                {
                    CalculateCurrents((Branch)comp);
                    continue;
                }

                else if (comp is ParallelBlock)
                {
                    CalculateCurrents((ParallelBlock)comp);
                    continue;
                }

            out1: ;
            }
        }


        public virtual void ExportToCSV(string FileName)
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
                foreach (var node in circuit.Nodes)
                {
                    results.Add(node.Key.ToString());
                }
                writer.WriteRow(results);
                results.Clear();

                foreach (var node in circuit.Nodes)
                {
                    results.Add(node.Value.Voltage.ToString());
                }
                writer.WriteRow(results);
                results.Clear();

                List<string> componames = new List<string>();
                ScanComponentBlockCurrents(results, componames, circuit);
                writer.WriteRow(componames);
                writer.WriteRow(results);

            }
       
        
        }

        /// <summary>
        /// escanea los componentes de un contenedor y almacena los nombres y corrientes de cada uno
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
                    results.Add(compo.current.ToString());
                    componames.Add(compo.Name);
                }
            }
        }
    }
}
