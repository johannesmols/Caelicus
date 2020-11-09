using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation.History
{
    public class VehicleStepState : Vehicle
    {
        public VehicleState State { get; set; }
        public string CurrentVertexPosition { get; set; }
        public string Target { get; set; }
        public HistoryCompletedOrder CurrentOrder { get; set; }
        public List<string> PathToTarget { get; set; }
        public double DistanceToTarget { get; set; }
        public double DistanceTraveled { get; set; }

        public VehicleStepState(Vehicle vehicle, VehicleState state, string currentVertexPosition, string target, HistoryCompletedOrder currentOrder, List<string> pathToTarget, double distanceToTarget, double distanceTraveled) : base(vehicle)
        {
            State = state;
            CurrentVertexPosition = currentVertexPosition;
            Target = target;
            CurrentOrder = currentOrder;
            PathToTarget = pathToTarget;
            DistanceToTarget = distanceToTarget;
            DistanceTraveled = distanceTraveled;
        }

        public VehicleStepState(Vehicle vehicle) : base(vehicle)
        {
        }

        public VehicleStepState()
        {

        }
    }
}
