using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Graph;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public class SimulationParameters
    {
        public Guid SimulationIdentifier { get; set; }

        public int RandomSeed { get; set; }

        public Graph<VertexInfo, EdgeInfo> Graph { get; set; } = new Graph<VertexInfo, EdgeInfo>();

        public List<VehicleInstance> Vehicles { get; set; } = new List<VehicleInstance>();

        public float SimulationSpeed { get; set; } = 1f;

        public float SimulationDuration { get; set; } = 1000f;

        public List<Order> Missions { get; set; }
    }
}
