using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
//using MathNet.Numerics.LinearAlgebra.Double;

namespace ElectricalAnalysis
{
    public class Circuit:Item
    {

        public List<ElectricComponent> Components { get; protected set; }
        public Dictionary<string,Node> Nodes { get; protected set; }
        public Boolean HasErrors { get; protected set; }

        public Vector<double> StaticResult;
        public Vector<double> StaticVector;
        public Matrix<double> StaticMatrix;

        public Circuit()
            : base()
        {
            Name = "Circuit" + ID.ToString();
            Components = new List<ElectricComponent>();
            Nodes = new Dictionary<string,Node>();
        }

        public void ReadCircuit(string CircuitName)
        {
            //Circuit cir = new Circuit();
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
                    ElectricComponent comp;
                    string[] comp1 = elemn[0].Split("_".ToCharArray());
                    switch (comp1[0].ToUpper())
                    {
                        case "R":
                            comp = new Resistor(comp1[1], elemn[3]);
                            break;
                        case "V":
                            comp = new VoltageGenerator(comp1[1], elemn[3]);
                            break;
                        case "I":
                            if (elemn[3] == "DC")
                                comp = new CurrentGenerator(comp1[1], elemn[4]);
                            else
                                //aun sin resolver para otros generadores
                                comp = new CurrentGenerator(comp1[1], elemn[4]);

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
                        n.IsReference = true;

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
                        n.IsReference = true;

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
            int fila = 0, columna = 0;
            List<Node> nodos = new List<Node>();

            foreach (var item in Nodes.Values)
            {
                if (!item.IsReference)
                    nodos.Add(item);
            }

            var v = Vector<double>.Build.Dense(Nodes.Count - 1);
            var A = Matrix<double>.Build.DenseOfArray(new double[Nodes.Count - 1,Nodes.Count - 1]);

            foreach (var nodo in nodos)
            {
                //busco generador de tension
                if (nodo.IsVoltageConnected)
                {
                    //ecuacion modificada de teorema de nodos
                    foreach (var compo in nodo.Components)
                    {
                        if (compo is VoltageGenerator)
                        {
                            if (compo.Nodes[0] == nodo)
                                v[fila] += compo.Value;
                            else
                                v[fila] -= compo.Value;

                            foreach (var item in compo.Nodes)
                            {
                                if (!item.IsReference)
                                {
                                    columna = nodos.IndexOf(item);
                                    if (item == nodo)
                                        A[fila, columna] = 1;
                                    else
                                        A[fila, columna] = -1;
                                }
                            }
                        }
                    }

                }
                else
                {
                    //ecuacion propia del teorema de nodos
                    foreach (var compo in nodo.Components)
                    {
                        if (compo is PasiveComponent)
                        {
                            columna = nodos.IndexOf(nodo);

                            A[fila, columna] += (1 / compo.Value);

                            if (nodo == compo.Nodes[0])
                                columna = nodos.IndexOf(compo.Nodes[1]);
                            else //if (nodo == compo.Nodes[1])
                                columna = nodos.IndexOf(compo.Nodes[0]);
                            
                            //nodo referencia no se suma
                            if (columna >= 0)
                                A[fila, columna] -= (1 / compo.Value);
    
                            
                        }
                    }
                   
                }

                fila++;
            }

            var x = A.Solve(v);

            StaticMatrix = A;
            StaticVector = v;
            StaticResult = x;

            return file;
        }


    }
}
