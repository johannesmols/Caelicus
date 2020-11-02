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

        public Vehicle VehicleTemplate { get; set; } = new Vehicle();

        public int NumberOfVehicles { get; set; } = 1;

        public float SimulationSpeed { get; set; } = 1f;

        public float SimulationDuration { get; set; } = 1000f;

        public List<Mission> Missions { get; set; }
    }
}
