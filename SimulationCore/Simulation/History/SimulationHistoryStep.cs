using System.Collections.Generic;

namespace SimulationCore.Simulation.History
{
    /// <summary>
    /// Stores a single step of a simulation
    /// </summary>
    public class SimulationHistoryStep
    {
        public int SimulationStep { get; set; }

        public List<VehicleStepState> Vehicles { get; set; }

        public List<HistoryOrder> OpenOrders { get; set; }

        public List<HistoryCompletedOrder> ClosedOrders { get; set; }

        public SimulationHistoryStep(int simulationStep, List<VehicleStepState> vehicles, List<HistoryOrder> openOrders, List<HistoryCompletedOrder> closedOrders)
        {
            SimulationStep = simulationStep;
            Vehicles = vehicles;
            OpenOrders = openOrders;
            ClosedOrders = closedOrders;
        }

        public SimulationHistoryStep()
        {
        }
    }
}
