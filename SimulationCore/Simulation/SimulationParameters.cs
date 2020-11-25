using System;
using Newtonsoft.Json;
using SimulationCore.Graph;
using SimulationCore.Models.Graph;
using SimulationCore.Models.Vehicles;

namespace SimulationCore.Simulation
{
    public class SimulationParameters
    {
        public Guid SimulationIdentifier { get; set; }

        public int RandomSeed { get; set; }

        public JsonGraphRootObject JsonGraph { get; set; }

        [JsonIgnore]
        public Graph<VertexInfo, EdgeInfo> Graph { get; set; } = new Graph<VertexInfo, EdgeInfo>();

        public Vehicle VehicleTemplate { get; set; }

        public int NumberOfVehicles { get; set; } = 1;

        public double SimulationSpeed { get; set; } = 1d;

        public int NumberOfOrders { get; set; } = 100;

        public Tuple<double, double> MinMaxPayload = Tuple.Create(0.1d, 5d);

        public bool LogIntermediateSteps { get; set; } = true;
    }
}
