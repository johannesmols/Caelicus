using System;
using System.Collections.Generic;
using BlazorApp.Services.GoogleMapsDistanceMatrix.Internal;
using Microsoft.AspNetCore.Components;
using SimulationCore.Graph;
using SimulationCore.Models.Graph;
using SimulationCore.Models.Vehicles;
using SimulationCore.Simulation;
using SimulationCore.Simulation.History;

namespace BlazorApp.Services
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

        public List<Tuple<string, Dictionary<Route, RouteStats>>> GraphStats { get; private set; } = new List<Tuple<string, Dictionary<Route, RouteStats>>>();

        public void UpdateGraphStats(ComponentBase source, List<Tuple<string, Dictionary<Route, RouteStats>>> graphStats)
        {
            GraphStats = graphStats;
            NotifyStateChanged(source, nameof(GraphStats));
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

        public bool OnlyDownloadLastStep { get; set; } = true;

        public void UpdateOnlyDownloadLastStep(ComponentBase source, bool onlyDownloadLastStep)
        {
            OnlyDownloadLastStep = onlyDownloadLastStep;
            NotifyStateChanged(source, nameof(OnlyDownloadLastStep));
        }


        // Orders
        public int NumberOfOrders { get; set; } = 100;

        public void UpdateNumberOfOrders(ComponentBase source, int numberOfOrders)
        {
            NumberOfOrders = numberOfOrders;
            NotifyStateChanged(source, nameof(NumberOfOrders));
        }

        public Tuple<double, double> MinMaxPayload { get; set; } = Tuple.Create(0.1d, 5d);

        public void UpdateMinMaxPayload(ComponentBase source, Tuple<double, double> minMaxPayload)
        {
            MinMaxPayload = minMaxPayload;
            NotifyStateChanged(source, nameof(MinMaxPayload));
        }


        // Simulation Results / History

        public SimulationHistory SimulationHistory { get; set; } = new SimulationHistory(new SimulationParameters());

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

        // Multiple simulation histories

        public List<SimulationHistory> SimulationHistories { get; set; } = new List<SimulationHistory>();

        public void UpdateSimulationHistoryList(ComponentBase source, List<SimulationHistory> simulationHistories)
        {
            SimulationHistories = simulationHistories;
            NotifyStateChanged(source, nameof(SimulationHistory));
        }

        // Events

        public event Action<ComponentBase, string> StateChanged;

        private void NotifyStateChanged(ComponentBase source, string property) => StateChanged?.Invoke(source, property);
    }
}
