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

        public Graph<VertexInfo, EdgeInfo> Graph { get; set; } = new Graph<VertexInfo, EdgeInfo>();

        public List<Tuple<Vehicle, bool>> Vehicles { get; set; } = new List<Tuple<Vehicle, bool>>();

        public float SimulationSpeed { get; set; } = 1f;
    }
}
