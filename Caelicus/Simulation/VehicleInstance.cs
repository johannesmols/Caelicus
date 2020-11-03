using System;
using System.Collections.Generic;
using System.Linq;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Helpers;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public enum VehicleState
    {
        Idle,
        MovingToTarget,
        MovingToBase
    }
    
    public class VehicleInstance : Vehicle
    {
        private Simulation Simulation { get; }

        public VehicleInstance(Simulation simulation, Vehicle vehicle, Vertex<VertexInfo, EdgeInfo> startingVertex) : base(vehicle)
        {
            Simulation = simulation;
            CurrentVertexPosition = startingVertex;
            State = VehicleState.Idle;
        }

        public VehicleState State { get; set; }
        public Vertex<VertexInfo, EdgeInfo> CurrentVertexPosition { get; set; } 
        public Vertex<VertexInfo, EdgeInfo> Target { get; set; }
        public CompletedOrder CurrentOrder { get; set; }

        private List<Vertex<VertexInfo, EdgeInfo>> PathToTarget { get; set; }
        private double DistanceToTarget { get; set; }
        private double DistanceTraveled { get; set; }

        public void AssignOrder(Order order)
        {
            CurrentOrder = new CompletedOrder(order);
            Target = order.Target;
            State = VehicleState.MovingToTarget;
            DistanceTraveled = 0d;

            var (path, distance) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, order.Start, order.Target);
            PathToTarget = path;
            DistanceToTarget = distance;
        }

        public void Advance()
        {
            if (State == VehicleState.Idle)
            {
                Simulation.ProgressReporter.Report(
                    new SimulationProgress(Simulation.Parameters.SimulationIdentifier,
                        $"Vehicle { GetHashCode() } idling at base station { CurrentVertexPosition.Info.Name }"));
            }
            else if (State == VehicleState.MovingToTarget)
            {
                if (CurrentOrder != null)
                {
                    // Calculate how many meters the vehicle travels in one simulation step
                    // Speed is in km/h, dividing by 3.6 gives it in m/s.
                    // The seconds per simulation step adjusts the calculation to the simulation speed (e.g. 1 second per step, 0.5 seconds per step, ...)
                    DistanceTraveled += (Speed / 3.6d) * Simulation.SecondsPerSimulationStep;

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier, 
                            $"Moving vehicle { GetHashCode() } to target { CurrentOrder.Target.Info.Name } from { CurrentOrder.Start.Info.Name } ({ DistanceTraveled / DistanceToTarget * 100d:n2}%)"));

                    // Arrived at target
                    if (DistanceTraveled >= DistanceToTarget)
                    {
                        // Close order
                        Simulation.ClosedOrders.Add(CurrentOrder);
                        CurrentOrder = null;

                        // Move to the nearest base station
                        State = VehicleState.MovingToBase;
                        CurrentVertexPosition = Target;
                        DistanceTraveled = 0d;
                        Target = GetNearestBaseStation(CurrentVertexPosition);

                        if (Target != null)
                        {
                            var (path, distance) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, Target);
                            PathToTarget = path;
                            DistanceToTarget = distance;
                        }
                    }
                }
            }
            else if (State == VehicleState.MovingToBase)
            {
                if (Target != null)
                {
                    DistanceTraveled += (Speed / 3.6d) * Simulation.SecondsPerSimulationStep;

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier,
                            $"Moving vehicle { GetHashCode() } to base { Target.Info.Name } from { CurrentVertexPosition.Info.Name } ({ DistanceTraveled / DistanceToTarget * 100d:n2}%)"));

                    // Arrived at base station
                    if (DistanceTraveled >= DistanceToTarget)
                    {
                        State = VehicleState.Idle;
                        CurrentVertexPosition = Target;
                        DistanceTraveled = 0d;
                    }
                }
            }
        }

        /// <summary>
        /// Get the nearest base station to the current position that has open orders available
        /// </summary>
        /// <param name="currentPosition">The vertex where the vehicle currently is</param>
        /// <returns></returns>
        private Vertex<VertexInfo, EdgeInfo> GetNearestBaseStation(Vertex<VertexInfo, EdgeInfo> currentPosition)
        {
            var nearestBaseStation = Simulation.Parameters.Graph
                .Where(x => x.Info.Type == VertexType.Base)
                .Where(x => Simulation.OpenOrders.Any(y => y.Start.Info == x.Info))
                .Select(x => Tuple.Create(GeographicalHelpers.CalculateGeographicalDistanceInMeters(currentPosition.Info.Position, x.Info.Position), x))
                .OrderBy(x => x.Item1)
                .FirstOrDefault();

            return nearestBaseStation?.Item2;
        }
    }
}