using System.Collections.Generic;
using System.Numerics;

namespace ElectricalAnalysis.Components.Controlled
{

    /// <summary>
    /// Defines a generic Controlled dipole
    /// </summary>
    public interface ControlledDipole
    {
        /// <summary>
        /// Allow to Controlled Dipole inform matrix
        /// coeficient in function of 
        /// </summary>
        /// <param name="node">node or component in equation</param>
        /// <param name="e">W or Time</param>
        /// <returns></returns>
        Complex ControllerEquationValue(object node, object e, bool isinput);

    }

    /// <summary>
    /// Voltage generator output
    /// </summary>
    public interface ControlledVoltageGenerator : ControlledDipole
    {
    }

    /// <summary>
    /// Current Generator output
    /// </summary>
    public interface ControlledCurrentGenerator : ControlledDipole
    {

    }

    /// <summary>
    /// Apply in Controlled Generator with short circuit inputs
    /// In this, current is sense
    /// </summary>
    public interface ControllerShortCircuit
    {
        List<NodeSingle> InputNodes { get; }
    }

    /// <summary>
    /// Apply in Controlled Generator with open circuit inputs
    /// In this, Voltage nodes is sense
    /// </summary>
    public interface ControllerOpenCircuit
    {
        List<NodeSingle> InputNodes { get; }
    }

}
