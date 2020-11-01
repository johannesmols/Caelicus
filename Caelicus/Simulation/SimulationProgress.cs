using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public class SimulationProgress
    {
        public Guid SimulationIdentifier { get; }
        public string Message { get; set; }

        public SimulationProgress(Guid identifier, string message)
        {
            SimulationIdentifier = identifier;
            Message = message;
        }
    }
}
