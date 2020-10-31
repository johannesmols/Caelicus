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

        public async Task Simulate()
        {
            Console.WriteLine("Simulating with speed " + _parameters.SimulationSpeed);
            await Task.Delay(1000);
        }
    }
}
