using System.Collections.Generic;

namespace ElectricalAnalysis.Components
{
    public  interface ComponentContainer
    {
        List<Dipole> Components { get; }
    }
}
