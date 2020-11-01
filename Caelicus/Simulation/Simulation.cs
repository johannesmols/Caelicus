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
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, "Starting simulation"));

            // TODO: Perform the actual simulation
            await Task.Delay(1000);

            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, "Finished simulation"));

            return new SimulationResult(_parameters.SimulationIdentifier, "Success");
        }
    }
}
