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
        public List<Func<Task<SimulationResult>>> Simulations { get; } = new List<Func<Task<SimulationResult>>>();
        private List<Task<SimulationResult>> _simulations;

        public void AddSimulation(SimulationParameters parameters)
        {
            var progress = new Progress<SimulationProgress>();
            progress.ProgressChanged += Test;
            Simulations.Add(() => new Simulation(parameters).Simulate(progress));
        }

        public async Task StartSimulations()
        {
            // Start simulations
            _simulations = Simulations.Select(sim => sim()).ToList();

            // Wait for all simulations to finish before continuing
            await Task.WhenAll(_simulations);

            Console.WriteLine("All tasks finished");

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
            // TODO
        }

        public async Task StopSimulations()
        {
            // TODO
        }

        public void SaveResults(List<SimulationResult> results)
        {
            // Save results to local storage
        }

        public void RemoveAllSimulations()
        {
            Simulations.Clear();
        }

        public void Test(object sender, SimulationProgress e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
