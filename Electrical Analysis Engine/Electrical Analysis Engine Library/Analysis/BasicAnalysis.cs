using CircuitMVVMBase;
using ElectricalAnalysis.Analysis.Solver;
using System;

namespace ElectricalAnalysis
{
    public abstract class BasicAnalysis:Item, ICloneable, IDescribible
    {

        //.tran<Tstep> <Tstop> [Tstart [dTmax]] [modifiers]
        //.ac<oct, dec, lin> <Nsteps> <StartFreq> <EndFreq>
        //.dc<srcnam> <Vstart> <Vstop> <Vincr> [<srcnam2> <Vstart2> <Vstop2> <Vincr2>]
        //.noise V(<out>[,<ref>]) <src> <oct, dec, lin> <Nsteps> <StartFreq> <EndFreq>
        //.tf V(<node>[, <ref>]) <source> OR I(<voltage source>) <source>
        //.op



        public virtual string ShortDescription { get; set; }
        public CircuitSolver Solver { get; protected set; }

        public abstract object Clone();

      

    }
}
