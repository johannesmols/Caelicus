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
        public List<Tuple<Func<Task<SimulationResult>>, Progress<SimulationProgress>, CancellationTokenSource>> Simulations { get; } = new List<Tuple<Func<Task<SimulationResult>>, Progress<SimulationProgress>, CancellationTokenSource>>();
        private List<Tuple<Task<SimulationResult>, CancellationTokenSource>> _simulations;

        public void AddSimulation(SimulationParameters parameters)
        {
            var progress = new Progress<SimulationProgress>();
            var cancellationTokenSource = new CancellationTokenSource();
            Simulations.Add(new Tuple<Func<Task<SimulationResult>>, Progress<SimulationProgress>, CancellationTokenSource>(() => new Simulation(parameters).Simulate(progress, cancellationTokenSource.Token), progress, cancellationTokenSource));
        }

        public async Task StartSimulations()
        {
            // Start simulations
            _simulations = Simulations.Select(sim => Tuple.Create(sim.Item1(), sim.Item3)).ToList();

            // Wait for all simulations to finish before continuing
            await Task.WhenAll(_simulations.Select(x => x.Item1));

            if (_simulations.All(s => s.Item1.IsCompletedSuccessfully))
            {
                SaveResults(_simulations.Select(x => x.Item1.Result).ToList());
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
            _simulations.ForEach(s => s.Item2.Cancel());
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
