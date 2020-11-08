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
        public Guid? CurrentVertexPosition { get; set; }
        public Guid? Target { get; set; }
        public HistoryCompletedOrder CurrentOrder { get; set; }
        public List<Guid> PathToTarget { get; set; }
        public double DistanceToTarget { get; set; }
        public double DistanceTraveled { get; set; }

        public VehicleStepState(Vehicle vehicle, VehicleState state, Guid? currentVertexPosition, Guid? target, HistoryCompletedOrder currentOrder, List<Guid> pathToTarget, double distanceToTarget, double distanceTraveled) : base(vehicle)
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
