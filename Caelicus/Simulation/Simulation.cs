using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public class Simulation
    {
        private readonly SimulationParameters _parameters;

        public Simulation(SimulationParameters parameters)
        {
            _parameters = parameters;
        }

        public async Task<SimulationResult> Simulate(IProgress<SimulationProgress> progress)
        {
            progress.Report(new SimulationProgress("Starting simulation"));
            await Task.Delay(1000);
            progress.Report(new SimulationProgress("Finished simulation"));
            return new SimulationResult("Success");
        }
    }
}
