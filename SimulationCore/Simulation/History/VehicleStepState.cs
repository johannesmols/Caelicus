using System.Collections.Generic;
using SimulationCore.Models.Vehicles;

namespace SimulationCore.Simulation.History
{
    public class VehicleStepState : Vehicle
    {
        public VehicleState State { get; set; }
        public List<string> PathToTarget { get; set; }
        public string CurrentVertexPosition { get; set; }
        public string CurrentTarget { get; set; }
        public double DistanceToCurrentTarget { get; set; }
        public double DistanceTraveled { get; set; }
        public double TotalTravelDistance { get; set; }
        public double TotalTravelTime { get; set; }
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
            double totalTravelDistance,
            double totalTravelTime,
            double currentFuelLoaded) : base(vehicle)
        {
            State = state;
            CurrentVertexPosition = currentVertexPosition;
            CurrentTarget = currentTarget;
            CurrentOrders = currentOrders;
            PathToTarget = pathToTarget;
            DistanceToCurrentTarget = distanceToCurrentTarget;
            DistanceTraveled = distanceTraveled;
            TotalTravelTime = totalTravelTime;
            TotalTravelDistance = totalTravelDistance;
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
