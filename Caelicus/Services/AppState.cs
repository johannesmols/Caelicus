using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Graph;
using Caelicus.Models;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;
using Caelicus.Simulation;
using Caelicus.Simulation.History;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Caelicus.Services
{
    public class AppState
    {
        // Graphs

        public List<JsonGraphRootObject> Graphs { get; private set; } = new List<JsonGraphRootObject>();

        public void UpdateGraphs(ComponentBase source, List<JsonGraphRootObject> graphs)
        {
            Graphs = graphs;
            NotifyStateChanged(source, nameof(Graphs));
        }


        // Vehicles

        public List<Tuple<Vehicle, bool, int, int, int>> Vehicles { get; set; } = new List<Tuple<Vehicle, bool, int, int, int>>();

        public void UpdateVehicles(ComponentBase source, List<Tuple<Vehicle, bool, int, int, int>> vehicles)
        {
            Vehicles = vehicles;
            NotifyStateChanged(source, nameof(Vehicles));
        }


        // Simulation
        public string SimulationUpdates { get; set; } = string.Empty;

        public void UpdateSimulationUpdates(ComponentBase source, string updates)
        {
            SimulationUpdates = updates;
            NotifyStateChanged(source, nameof(SimulationUpdates));
        }


        // Orders
        public int NumberOfOrders { get; set; } = 100;

        public void UpdateNumberOfOrders(ComponentBase source, int numberOfOrders)
        {
            NumberOfOrders = numberOfOrders;
            NotifyStateChanged(source, nameof(NumberOfOrders));
        }


        public List<SimulationHistory> SimulationHistories { get; set; } = new List<SimulationHistory>();

        public void UpdateSimulationHistoryList(ComponentBase source, List<SimulationHistory> simulationHistories)
        {
            SimulationHistories = simulationHistories;
            NotifyStateChanged(source, nameof(SimulationHistory));
        }

        // Simulation Results / History

        public SimulationHistory SimulationHistory { get; set; } = new SimulationHistory(new SimulationParameters(), new List<VehicleInstance>());

        public void UpdateSimulationHistory(ComponentBase source, SimulationHistory simulationHistory)
        {
            SimulationHistory = simulationHistory;
            NotifyStateChanged(source, nameof(SimulationHistory));
        }

        public int HistorySimulationStep { get; set; }

        public void UpdateSimulationStep(ComponentBase source, int step)
        {
            HistorySimulationStep = step;
            NotifyStateChanged(source, nameof(HistorySimulationStep));
        }

        public Graph<VertexInfo, EdgeInfo> HistoryGraph { get; set; } = new Graph<VertexInfo, EdgeInfo>();

        public void UpdateHistoryGraph(ComponentBase source, Graph<VertexInfo, EdgeInfo> graph)
        {
            HistoryGraph = graph;
            NotifyStateChanged(source, nameof(HistoryGraph));
        }

        // Events

        public event Action<ComponentBase, string> StateChanged;

        private void NotifyStateChanged(ComponentBase source, string property) => StateChanged?.Invoke(source, property);
    }
}
