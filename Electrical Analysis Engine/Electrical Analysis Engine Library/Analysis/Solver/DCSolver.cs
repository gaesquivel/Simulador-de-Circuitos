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
                    if (!nodosnormales.Contains(other))
                        nodosnormales.Add(other);
                }
                yaanalizados.Add(compo);
            }
            else
            {
                ramas.Add(br);
                Node other = br.OtherNode(nodo);
                if (!nodosnormales.Contains(other))
                    nodosnormales.Add(other);
                yaanalizados.AddRange(br.Components);
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
            int fila = 0, columna = 0;
            List<Node> nodos = new List<Node>();

            nodos.AddRange(cir.Nodes.Values);
            nodos.Remove(cir.Reference);
            //foreach (var nodo in cir.Nodes.Values)
            //{
            //    //creo una lista de nodos sin el nodo referencia
            //    if (!nodo.IsReference)
            //        nodos.Add(nodo);
            //}

            List<Node> nodosnorton = new List<Node>();

            foreach (var nodo in nodos)
            {
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode ||
                    nodo.TypeOfNode == Node.NodeType.VoltageLinkedNode)
                    nodosnorton.Add(nodo);
            
            }

            var v = Vector<double>.Build.Dense(nodosnorton.Count);
            var A = Matrix<double>.Build.DenseOfArray(new double[nodosnorton.Count, nodosnorton.Count]);

            #region Calculo de tensiones de nodos
            foreach (var nodo in nodosnorton)
            {
                columna = fila = nodosnorton.IndexOf(nodo);
                if (nodo.TypeOfNode == Node.NodeType.MultibranchCurrentNode)
                {
                    foreach (var rama in nodo.Components)
                    {
                        if (rama is Branch || rama is PasiveComponent)
                        {
                            v[fila] += rama.NortonCurrent(nodo).Real;
                            A[fila, columna] += 1 / rama.Impedance().Real;
                        }
                    }
                }
                else if (nodo.TypeOfNode == Node.NodeType.VoltageDivideNode)
                {
                    Dipole compo1 = nodo.Components[0];
                    Dipole compo2 = nodo.Components[1];
                    Node nodo1 = compo1.OtherNode(nodo);
                    Node nodo2 = compo2.OtherNode(nodo);
                    Complex32  z1, z2;
                    double v1, v2;
                    v1 = nodo1.Voltage.Real;
                    v2 = nodo2.Voltage.Real;
                    z1 = compo1.Impedance();
                    z2 = compo2.Impedance();

                    if (nodo1.IsVoltageKnowed)
                        v[fila] += v1 * z1.Real;
                    else
                    {
                        columna = nodosnorton.IndexOf(nodo1);
                        A[fila, columna] -= v1 * z1.Real;
                    }
                    if (nodo2.IsVoltageKnowed)
                        v[fila] += v2 * z2.Real;
                    else
                    {
                        columna = nodosnorton.IndexOf(nodo2);
                        A[fila, columna] -= v2 * z2.Real;
                    }
                    columna = nodosnorton.IndexOf(nodo);
                    A[fila, columna] += (z1 + z2).Real;
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
            #endregion

            #region almacenamiento temporal

            foreach (var nodo in nodosnorton)
            {
                fila = nodosnorton.IndexOf(nodo);
                nodo.Voltage = new Complex32((float)x[fila], 0);
            }

            #endregion
            

            return true;
        }
    }
}
