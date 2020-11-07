using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation.History
{
    public class VehicleStepState : Vehicle
    {
        public VehicleState State { get; }
        public Guid CurrentVertexPosition { get; }
        public Guid Target { get; }
        public CompletedOrder CurrentOrder { get; }
        public List<Guid> PathToTarget { get; }
        public double DistanceToTarget { get; }
        public double DistanceTraveled { get; }

        public VehicleStepState(Vehicle vehicle, VehicleState state, Guid currentVertexPosition, Guid target, CompletedOrder currentOrder, List<Guid> pathToTarget, double distanceToTarget, double distanceTraveled) : base(vehicle)
        {
            State = state;
            CurrentVertexPosition = currentVertexPosition;
            Target = target;
            CurrentOrder = currentOrder;
            PathToTarget = pathToTarget;
            DistanceToTarget = distanceToTarget;
            DistanceTraveled = distanceTraveled;
        }
    }
}
