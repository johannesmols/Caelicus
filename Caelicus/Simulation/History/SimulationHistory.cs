using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation.History
{
    /// <summary>
    /// Stores the entire history of a simulation and all its steps
    /// </summary>
    public class SimulationHistory
    {
        public SimulationParameters Parameters { get; }

        public readonly List<SimulationHistoryStep> Steps = new List<SimulationHistoryStep>();

        public SimulationHistory(SimulationParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
