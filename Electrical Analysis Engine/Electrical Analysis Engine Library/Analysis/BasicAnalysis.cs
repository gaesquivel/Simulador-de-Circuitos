﻿using ElectricalAnalysis.Analysis.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public abstract class BasicAnalysis:Item, ICloneable
    {
        public CircuitSolver Solver { get; protected set; }

        public abstract object Clone();
        


    }
}
