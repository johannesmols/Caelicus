using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation.History
{
    /// <summary>
    /// Stores a single step of a simulation
    /// </summary>
    public class SimulationHistoryStep
    {
        public int SimulationStep { get; }

        public List<VehicleInstance> Vehicles { get; }

        public List<Order> OpenOrders { get; }

        public List<CompletedOrder> ClosedOrders { get; }

        public SimulationHistoryStep(int simulationStep, List<VehicleInstance> vehicles, List<Order> openOrders, List<CompletedOrder> closedOrders)
        {
            SimulationStep = simulationStep;
            Vehicles = vehicles;
            OpenOrders = openOrders;
            ClosedOrders = closedOrders;
        }
    }
}
