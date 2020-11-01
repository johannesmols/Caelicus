using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public class SimulationResult
    {
        public string Result { get; set; }

        public SimulationResult(string result)
        {
            Result = result;
        }
    }
}
