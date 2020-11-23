using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Caelicus.Simulation.History;

namespace Caelicus.Simulation
{
    public class SimulationManager
    {
        public List<Tuple<Func<Task<SimulationHistory>>, Progress<SimulationProgress>, CancellationTokenSource>> Simulations { get; } = new List<Tuple<Func<Task<SimulationHistory>>, Progress<SimulationProgress>, CancellationTokenSource>>();
        private List<Tuple<Task<SimulationHistory>, CancellationTokenSource>> _simulations;

        public void AddSimulation(SimulationParameters parameters)
        {
            var progress = new Progress<SimulationProgress>();
            var cancellationTokenSource = new CancellationTokenSource();
            Simulations.Add(new Tuple<Func<Task<SimulationHistory>>, Progress<SimulationProgress>, CancellationTokenSource>(() => new Simulation(parameters).Simulate(progress, cancellationTokenSource.Token), progress, cancellationTokenSource));
        }

        public async Task<List<SimulationHistory>> StartSimulations()
        {
            // Start simulations
            _simulations = Simulations.Select(sim => Tuple.Create(sim.Item1(), sim.Item3)).ToList();

            return await WaitForResults();
        }

        private async Task<List<SimulationHistory>> WaitForResults()
        {
            // Wait for all simulations to finish before continuing
            await Task.WhenAll(_simulations.Select(x => x.Item1));

            var results = new List<SimulationHistory>();

            if (_simulations.All(s => s.Item1.IsCompletedSuccessfully))
            {
                results.AddRange(_simulations.Select(x => x.Item1.Result).ToList());
                RemoveAllSimulations();
            }
            else
            {
                Console.WriteLine("Not all simulations were completed successfully.");
            }

            return results;
        }

        public async Task<List<SimulationHistory>> StopSimulations()
        {
            _simulations.ForEach(s => s.Item2.Cancel());
            return await WaitForResults();
        }

        public void RemoveAllSimulations()
        {
            Simulations.Clear();
        }
    }
}
