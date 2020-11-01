using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public class SimulationResult
    {
        public Guid SimulationIdentifier { get; }
        public string Result { get; set; }

        public SimulationResult(Guid identifier, string result)
        {
            SimulationIdentifier = identifier;
            Result = result;
        }
    }
}
