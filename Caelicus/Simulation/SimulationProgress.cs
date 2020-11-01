using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public class SimulationProgress
    {
        public string Message { get; set; }

        public SimulationProgress(string message)
        {
            Message = message;
        }
    }
}
