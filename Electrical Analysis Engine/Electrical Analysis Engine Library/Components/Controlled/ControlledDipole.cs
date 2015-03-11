using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis.Components.Controlled
{
    public interface ControlledDipole
    {
    }

    /// <summary>
    /// Voltage generator 
    /// </summary>
    public interface ControlledVoltageGenerator : ControlledDipole
    {
        List<Node> InputNodes { get; }
    }

    public interface ControlledCurrentGenerator : ControlledDipole
    {

    }

    public interface ControllerShortCircuit
    {


    }

    public interface ControllerOpenCircuit
    {

    }

}
