using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimulationCore.Enums;
using SimulationCore.Graph;
using SimulationCore.Models.Graph;
using SimulationCore.Simulation.History;

namespace SimulationCore.Simulation
{
    public class Simulation
    {
        public IProgress<SimulationProgress> ProgressReporter { get; private set; }
        public readonly SimulationParameters Parameters;
        public readonly SimulationHistory SimulationHistory;
        public List<VehicleInstance> Vehicles { get; } = new List<VehicleInstance>();
        public List<Order> OpenOrders { get; set; } = new List<Order>();
        public List<CompletedOrder> ClosedOrders { get; set; } = new List<CompletedOrder>();

        public readonly double SecondsPerSimulationStep;
        public int SimulationStep { get; private set; }


        /// <summary>
        /// Generates list of vehicles and orders based on the simulation parameters
        /// </summary>
        /// <param name="parameters">simulation parameters</param>
        public Simulation(SimulationParameters parameters)
        {
            Parameters = parameters;
            SimulationHistory = new SimulationHistory(parameters);
            SecondsPerSimulationStep = 1d / Parameters.SimulationSpeed;

            var allBases = Parameters.Graph.Vertices.Where(v => v.Info.Type == VertexType.Base).ToList();
            var allTargets = Parameters.Graph.Vertices.Where(v => v.Info.Type == VertexType.Target).ToList();

            // Equally split vehicles up to base stations
            var currentBaseIndex = 0;
            for (var i = 0; i < Parameters.NumberOfVehicles; i++)
            {
                if (currentBaseIndex == allBases.Count)
                {
                    currentBaseIndex = 0;
                }

                Vehicles.Add(new VehicleInstance(this, Parameters.VehicleTemplate,
                    allBases[new Random(Parameters.RandomSeed + i).Next(allBases.Count)]));
                currentBaseIndex++;
            }

            // Generate random orders
            for (var i = 0; i < Parameters.NumberOfOrders; i++)
            {
                // Multiplying the random seed + i with a large number because a change of only 1 per iteration produces very similar results when calculating random values
                OpenOrders.Add(new Order(
                    allBases[new Random((Parameters.RandomSeed + i) * 133742069).Next(allBases.Count)],
                    allTargets[new Random((Parameters.RandomSeed + i) * 133742069).Next(allTargets.Count)],
                    new Random((Parameters.RandomSeed + i) * 133742069).NextDouble() *
                    (Parameters.MinMaxPayload.Item2 - Parameters.MinMaxPayload.Item1) +
                    Parameters.MinMaxPayload.Item1));
            }
        }

        /// <summary>
        /// Main thread of running the simulation, posting updates, and finally returning the results
        /// </summary>
        /// <param name="progress">Can be used to send status updates back to the UI</param>
        /// <param name="cancellationToken">Can be used to cancel the operation from the UI</param>
        /// <returns></returns>
        public async Task<SimulationHistory> Simulate(IProgress<SimulationProgress> progress,
            CancellationToken cancellationToken)
        {
            ProgressReporter = progress;
            ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier,
                $"Starting simulation with  {Parameters.NumberOfVehicles} {Parameters.VehicleTemplate.Name}"));

            while (!IsDone())
            {
                if (Parameters.LogIntermediateSteps)
                {
                    ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier,
                        $"Simulating at step {SimulationStep}: " +
                        $"({OpenOrders.Count} open orders, " +
                        $"{ClosedOrders.Count} closed orders, " +
                        $"{Vehicles.Where(v => v.CurrentOrders != null && v.State == VehicleState.MovingToTarget).ToList().Sum(v => v.CurrentOrders.Count)} orders in progress, " +
                        $"{Vehicles.Where(v => v.CurrentOrders != null && v.State == VehicleState.PickingUpOrder).ToList().Sum(v => v.CurrentOrders.Count)} in pickup)"));
                }

                // Record the current state of the simulation
                RecordSimulationStep();

                // Advance the simulation by one step
                Advance();

                // Wait for an amount of time corresponding to the simulation speed (e.g. speed of 1 = 1 step per second, speed of 2 = 2 steps per second, ...)
                if (Parameters.SimulationSpeed != 0d)
                {
                    await Task.Delay((int) (SecondsPerSimulationStep * 1000));
                }

                // Use this snippet to repeatedly check for cancellation in each iteration of the simulation
                if (cancellationToken.IsCancellationRequested)
                {
                    ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier,
                        $"Stopped simulation with {Parameters.NumberOfVehicles} {Parameters.VehicleTemplate.Name}"));
                    return SimulationHistory;
                }
            }

            // Record last step as well
            RecordSimulationStep();
            ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier,
                $"Finished simulation with {Parameters.NumberOfVehicles} {Parameters.VehicleTemplate.Name}"));
            return SimulationHistory;
        }

        /// <summary>
        /// Determines whether all orders have been fulfilled successfully
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            if (OpenOrders.Count == 0 && Vehicles.All(v => v.State == VehicleState.Idle))
            {
                return true;
            }

            if (OpenOrders.Count > 0)
            {
                if (Vehicles.All(v => v.State == VehicleState.Idle && v.FindOptimalOrders()?.Count == 0))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Advance the simulation by a single step
        /// </summary>
        public void Advance()
        {
            // Advance all vehicles and their assigned orders
            Vehicles.ForEach(v => v.AdvanceNew());

            SimulationStep++;
        }

        /// <summary>
        /// Get the nearest open order available for pickup from the current location
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public Tuple<Order, Vertex<VertexInfo, EdgeInfo>> GetNearestOpenOrder(VehicleInstance vehicle)
        {
            var nearestBaseStation = GetNearestBaseStationWithOpenOrder(vehicle);
            var order = OpenOrders.FirstOrDefault(o => o.Start == nearestBaseStation);

            return Tuple.Create(order, nearestBaseStation);
        }

        /// <summary>
        /// Get the nearest base station to the current position that has open orders available
        /// </summary>
        /// <param name="vehicle">The vehicle</param>
        /// <returns></returns>
        public Vertex<VertexInfo, EdgeInfo> GetNearestBaseStationWithOpenOrder(VehicleInstance vehicle)
        {
            var nearestBaseStation = Parameters.Graph
                .Where(x => x.Info.Type == VertexType.Base)
                .Where(x => OpenOrders.Any(y =>
                    y.Start.Info == x.Info &&
                    Parameters.Graph.FindShortestPath(Parameters.Graph, y.Start, y.Target, vehicle.TravelMode).Item2 <=
                    vehicle.GetMaximumTravelDistance(y.PayloadWeight, vehicle.CurrentFuelLoaded)))
                .Select(x =>
                    Tuple.Create(
                        Parameters.Graph.FindShortestPath(Parameters.Graph, vehicle.CurrentVertexPosition, x,
                            vehicle.TravelMode).Item2, x))
                .OrderBy(x => x.Item1)
                .FirstOrDefault();

            return nearestBaseStation?.Item2;
        }

        /// <summary>
        /// Record the current state of the simulation for analysis purposes
        /// </summary>
        private void RecordSimulationStep()
        {
            var simHistoryStep = new SimulationHistoryStep()
            {
                SimulationStep = SimulationStep,
                Vehicles = new List<VehicleStepState>(),
                OpenOrders = new List<HistoryOrder>(),
                ClosedOrders = new List<HistoryCompletedOrder>()
            };

            foreach (var vehicle in Vehicles)
            {
                if (vehicle != null)
                {
                    var vehicleState = new VehicleStepState(vehicle.Vehicle)
                    {
                        State = vehicle.State,
                        PathToTarget = vehicle.PathToTarget?.Select(p => p.Info.Name).ToList(),
                        CurrentVertexPosition = vehicle.CurrentVertexPosition?.Info?.Name,
                        CurrentTarget = vehicle.CurrentTarget?.Info?.Name,
                        CurrentOrders = new List<HistoryCompletedOrder>(vehicle.CurrentOrders?.Select(o =>
                                                                            new HistoryCompletedOrder(new HistoryOrder
                                                                                {
                                                                                    Start = o?.Start?.Info?.Name,
                                                                                    Target = o?.Target?.Info?.Name,
                                                                                    PayloadWeight = o?.PayloadWeight
                                                                                },
                                                                                o?.DeliveryTime,
                                                                                o?.DeliveryDistance,
                                                                                o?.DeliveryCost,
                                                                                o?.DeliveryPath
                                                                                    ?.Select(p => p.Info.Name)
                                                                                    .ToList())) ??
                                                                        Array.Empty<HistoryCompletedOrder>()),
                        DistanceToCurrentTarget = vehicle.DistanceToCurrentTarget,
                        DistanceTraveled = vehicle.DistanceTraveled,
                        TotalTravelDistance = vehicle.TotalTravelDistance,
                        TotalTravelTime = vehicle.TotalTravelTime,
                        CurrentFuelLoaded = vehicle.CurrentFuelLoaded
                    };

                    simHistoryStep.Vehicles.Add(vehicleState);
                }
            }

            foreach (var openOrder in OpenOrders)
            {
                if (openOrder != null)
                {
                    var order = new HistoryOrder()
                    {
                        Start = openOrder.Start?.Info.Name,
                        Target = openOrder.Target?.Info.Name,
                        PayloadWeight = openOrder.PayloadWeight
                    };

                    simHistoryStep.OpenOrders.Add(order);
                }
            }

            foreach (var closedOrder in ClosedOrders)
            {
                if (closedOrder != null)
                {
                    var order = new HistoryCompletedOrder(new HistoryOrder(closedOrder.Order?.Start?.Info.Name,
                        closedOrder.Order?.Target?.Info.Name, closedOrder.Order?.PayloadWeight))
                    {
                        DeliveryDistance = closedOrder.DeliveryDistance,
                        DeliveryTime = closedOrder.DeliveryTime,
                        DeliveryPath = closedOrder.DeliveryPath?.Select(p => p.Info.Name).ToList(),
                        DeliveryCost = closedOrder.DeliveryCost,
                    };

                    simHistoryStep.ClosedOrders.Add(order);
                }
            }

            SimulationHistory.Steps.Add(simHistoryStep);
        }
    }
}