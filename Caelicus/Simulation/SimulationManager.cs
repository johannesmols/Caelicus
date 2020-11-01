using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public class SimulationManager
    {
        public List<Tuple<Func<Task<SimulationResult>>, Progress<SimulationProgress>>> Simulations { get; } = new List<Tuple<Func<Task<SimulationResult>>, Progress<SimulationProgress>>>();
        private List<Task<SimulationResult>> _simulations;

        public void AddSimulation(SimulationParameters parameters)
        {
            var progress = new Progress<SimulationProgress>();
            Simulations.Add(new Tuple<Func<Task<SimulationResult>>, Progress<SimulationProgress>>(() => new Simulation(parameters).Simulate(progress), progress));
        }

        public async Task StartSimulations()
        {
            // Start simulations
            _simulations = Simulations.Select(sim => sim.Item1()).ToList();

            // Wait for all simulations to finish before continuing
            await Task.WhenAll(_simulations);

            if (_simulations.All(s => s.IsCompletedSuccessfully))
            {
                SaveResults(_simulations.Select(x => x.Result).ToList());
                RemoveAllSimulations();
            }
            else
            {
                Console.WriteLine("Not all simulations were completed successfully.");
            }
        }

        public async Task PauseSimulations()
        {
            // TODO implement pause
        }

        public async Task StopSimulations()
        {
            // TODO implement stop
        }

        public void SaveResults(List<SimulationResult> results)
        {
            // TODO Save results to local storage
        }

        public void RemoveAllSimulations()
        {
            Simulations.Clear();
        }
    }
}
