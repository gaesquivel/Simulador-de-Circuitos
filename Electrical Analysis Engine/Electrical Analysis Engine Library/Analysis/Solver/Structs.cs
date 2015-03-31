


using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using System.Collections.Generic;


public class SpecialComponentInfo
{
    public Dipole Component;
    public List<Node> ImportantInputNodes { get; private set;}
    public List<Node> ImportantOutputNodes { get; private set; }

    public SpecialComponentInfo(Dipole comp)
    {
        Component = comp;
        ImportantInputNodes = new List<Node>();
        ImportantOutputNodes = new List<Node>();
    }
}

public class SolveInfo
{
    public List<Node> calculablenodes { get; private set; }
    public List<Node> calculatednodes { get; private set; }
    public List<Node> nortonnodes { get; private set; }
    public List<SpecialComponentInfo> specialcomponents { get; private set; }
    public List<Branch> ramas { get; private set; }
    public List<SuperNode> SuperNodes { get; private set; }


    public int MatrixDimension
    {
        get
        {
            int n = 0;
            n = nortonnodes.Count;
            foreach (var item in specialcomponents)
            {
                n += item.ImportantOutputNodes.Count;
            }
            return n;
        }
    }

    public SolveInfo()
    {
        calculablenodes = new List<Node>();
        calculatednodes = new List<Node>();
        nortonnodes = new List<Node>();
        specialcomponents = new List<SpecialComponentInfo>();
        ramas = new List<Branch>();
        SuperNodes = new List<SuperNode>();
    }   

}
