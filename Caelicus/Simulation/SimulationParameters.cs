using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public class SimulationParameters
    {
        /// <summary>
        /// Graph to use in the simulation
        /// </summary>
        public JsonGraphRootObject Graph { get; set; } = new JsonGraphRootObject();

        /// <summary>
        /// List of all vehicles in the simulation, including their starting base station and quantity at that base station
        /// </summary>
        public List<Tuple<Vehicle, string, int>> Vehicles { get; set; } = new List<Tuple<Vehicle, string, int>>();
    }
}
