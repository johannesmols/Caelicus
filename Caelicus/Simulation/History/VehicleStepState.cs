using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Graph;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation.History
{
    public class VehicleStepState : Vehicle
    {
        public VehicleState State { get; set; }
        public List<string> PathToTarget { get; set; }
        public string CurrentVertexPosition { get; set; }
        public string CurrentTarget { get; set; }
        public double DistanceToCurrentTarget { get; set; }
        public double DistanceTraveled { get; set; }
        public List<HistoryCompletedOrder> CurrentOrders { get; set; }
        public double CurrentFuelLoaded { get; set; }

        public VehicleStepState(
            Vehicle vehicle, 
            VehicleState state, 
            string currentVertexPosition, 
            string currentTarget, 
            List<HistoryCompletedOrder> currentOrders, 
            List<string> pathToTarget, 
            double distanceToCurrentTarget, 
            double distanceTraveled, 
            double currentFuelLoaded) : base(vehicle)
        {
            State = state;
            CurrentVertexPosition = currentVertexPosition;
            CurrentTarget = currentTarget;
            CurrentOrders = currentOrders;
            PathToTarget = pathToTarget;
            DistanceToCurrentTarget = distanceToCurrentTarget;
            DistanceTraveled = distanceTraveled;
            CurrentFuelLoaded = currentFuelLoaded;
        }

        public VehicleStepState(Vehicle vehicle) : base(vehicle)
        {
        }

        public VehicleStepState()
        {

        }
    }
}
