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

        public async Task<SimulationResult> Simulate(IProgress<SimulationProgress> progress, CancellationToken cancellationToken)
        {
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Starting simulation with  { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));

            // TODO: Perform the actual simulation
            await Task.Delay(1000);

            // Use this snippet to repeatedly check for cancellation in each iteration of the simulation
            if (cancellationToken.IsCancellationRequested)
            {
                progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Stopped simulation with { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));
                throw new TaskCanceledException();
            }

            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Finished simulation with { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));

            return new SimulationResult(_parameters.SimulationIdentifier, "Success");
        }
    }
}
