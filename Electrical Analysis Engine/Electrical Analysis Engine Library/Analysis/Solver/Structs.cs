using ElectricalAnalysis.Analysis.Solver;
using ElectricalAnalysis.Components;
using ElectricalAnalysis.Components.Controlled;
using System;
using System.Collections.Generic;


public class SolveInfo
{

    #region listas

    /// <summary>
    /// List of all circuit nodes, excluding earth
    /// The node Index is used to matrix equations
    /// </summary>
    public List<NodeSingle> Nodes { get; private set; }

    /// <summary>
    /// List of nodes must be ignore (for some reason)
    /// </summary>
    public List<NodeSingle> IgnoreNodes { get; private set; }

    /// <summary>
    /// List of component must be ignore in first step
    /// </summary>
    public List<Dipole> IgnoreComponents { get; private set; }

    /// <summary>
    /// List of all nodes conected to earth with a voltage generator
    /// This Voltage could be calculated directly
    /// </summary>
    public List<NodeSingle> AutoCalculableNodes { get; private set; }

    /// <summary>
    /// List of all single aislated circuit nodes, excluding
    /// Fixed voltage autocalculable nodes
    /// </summary>
    public List<NodeSingle> NortonNodes { get; private set; }

    /// <summary>
    /// List of all supernodes
    /// </summary>
    public List<SuperNode> SuperNodes { get; private set; }

    /// <summary>
    /// List of all nodes in supernodes
    /// </summary>
    public List<NodeSingle> NodesInSupernodes { get; private set; }

    /// <summary>
    /// List of all voltage generator in supernodes
    /// </summary>
    public List<Dipole> GeneratorInSupernodes { get; private set; }


    /// <summary>
    /// List of all Special Components
    /// </summary>
    public List<ControlledDipole> SpecialComponents { get; private set; }

    /// <summary>
    /// List of all Current output controlled Generator
    /// </summary>
    public List<Dipole> SpecialOutputCurrentComponents { get; private set; }

    /// <summary>
    /// List of all Current input controlled Generator
    /// </summary>
    public List<Dipole> SpecialInputCurrentComponents { get; private set; }

    /// <summary>
    /// List of all matrix reelevant special component nodes
    /// </summary>
    public List<NodeSingle> SpecialComponentNodes { get; private set; }

    #endregion

    /// <summary>
    /// Number of matrix variable count
    /// </summary>
    public int ColumnsCount
    {
        get
        {
            int n = 0;
            
            n = NortonNodes.Count + SuperNodes.Count  +
                SpecialOutputCurrentComponents.Count + SpecialComponentNodes.Count +
                SpecialInputCurrentComponents.Count + GeneratorInSupernodes.Count;
            return n;
        }
    }

    /// <summary>
    /// Number of matrix equation count
    /// </summary>
    public int RowCount
    {
        get
        {
            int n = 0;
            n = NortonNodes.Count + SuperNodes.Count + 
                GeneratorInSupernodes.Count + SpecialComponents.Count;
            
            return n;
        }
    }

    public Circuit Circuit { get; protected set; }

    /// <summary>
    /// Returns Value index of node, current (dipole) in the matrix system equation
    /// </summary>
    /// <param name="e">node, supernode, or (controled or controler) current</param>
    /// <returns></returns>
    public int ValueIndexOf(object e)
    {
        if (e is NodeSingle)
        {
            return AutoCalculableNodes.IndexOf((NodeSingle)e);
        }
        throw new NotImplementedException();
    }

    public int ColumnIndexOf(Dipole comp, Node node)
    {
        if (comp is ControllerOpenCircuit)
        {
            return NortonNodes.Count + SpecialInputCurrentComponents.Count 
                //+ SpecialOutputCurrentComponents.Count 
                + SpecialComponentNodes.Count //+ GeneratorInSupernodes.Count
                   + SpecialOutputCurrentComponents.IndexOf(comp);
        }
        else if (comp is ControllerShortCircuit)
        {
            return Nodes.Count + GeneratorInSupernodes.Count
                   + SpecialComponents.IndexOf((ControlledDipole)comp);
        }
        else if (comp is ControlledDipole)
        {
            return Nodes.Count + GeneratorInSupernodes.Count +
                SpecialComponents.IndexOf((ControlledDipole)comp);
        }
        else if (comp is Dipole)
        {
            return Nodes.Count + SpecialComponents.IndexOf((ControlledDipole)comp);
        }
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns Column index of node, current (dipole) in the matrix system equation
    /// </summary>
    /// <param name="e">node, supernode, or (controled or controler) current</param>
    /// <returns></returns>
    public int ColumnIndexOf(NodeSingle e)
    {
        if (NortonNodes.Contains(e))
            return NortonNodes.IndexOf(e);
        else if (NodesInSupernodes.Contains(e))
            return NortonNodes.Count + NodesInSupernodes.IndexOf(e);
        else if (SpecialComponentNodes.Contains(e))
            return NortonNodes.Count + NodesInSupernodes.Count //+ SpecialOutputCurrentComponents.Count
                 + SpecialComponentNodes.IndexOf(e);

        else
            throw new NotImplementedException();
    }

    /// <summary>
    /// Returns index of node, current (dipole) in the matrix system equation
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public int RowIndexOf(object e)
    {
        if (e is NodeSingle)
        {
            return NortonNodes.IndexOf((NodeSingle)e);
        }
        else if (e is SuperNode)
        {
            return NortonNodes.Count + SuperNodes.IndexOf((SuperNode)e);
        }
        else if (e is ControlledDipole)
        {
            return NortonNodes.Count + SuperNodes.Count + GeneratorInSupernodes.Count +
                SpecialComponents.IndexOf((ControlledDipole)e);
        }
        else if (e is Dipole)
        {
            return NortonNodes.Count + SuperNodes.Count + GeneratorInSupernodes.IndexOf((Dipole)e);
        }
        throw new NotImplementedException();
        //return -1;
    }
   

    public SolveInfo(Circuit cir)
    {
        Circuit = cir;
        Nodes = new List<NodeSingle>();
        NortonNodes = new List<NodeSingle>();
        SpecialComponentNodes = new List<NodeSingle>();
        SpecialComponents = new List<ControlledDipole>();
        SpecialOutputCurrentComponents= new List<Dipole>();
        SpecialInputCurrentComponents = new List<Dipole>();
        SuperNodes = new List<SuperNode>();
        NodesInSupernodes = new List<NodeSingle>();
        AutoCalculableNodes = new List<NodeSingle>();
        GeneratorInSupernodes = new List<Dipole>();
        IgnoreComponents = new List<Dipole>();
        IgnoreNodes= new List<NodeSingle>();
    }

}
