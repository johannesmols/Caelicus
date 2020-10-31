using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caelicus.Simulation
{
    public static class SimulationManager
    {
        public static List<Task> Simulations = new List<Task>();

        public static void AddSimulation(SimulationParameters parameters)
        {
            Simulations.Add(new Task(async () => await new Simulation(parameters).Simulate()));
        }

        public static async Task StartSimulations()
        {
            Simulations.ForEach(s => s.Start());
            await Task.WhenAll(Simulations);

            Console.WriteLine("All tasks finished");
        }
    }
}
