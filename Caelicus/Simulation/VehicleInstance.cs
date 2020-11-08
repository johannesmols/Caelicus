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
        PickingUpOrder
    }
    
    public class VehicleInstance : Vehicle
    {
        private Simulation Simulation { get; }

        public VehicleInstance(Simulation simulation, Vehicle vehicle, Vertex<VertexInfo, EdgeInfo> startingVertex) : base(vehicle)
        {
            Simulation = simulation;
            Vehicle = vehicle;
            CurrentVertexPosition = startingVertex;
            State = VehicleState.Idle;
        }

        public Vehicle Vehicle { get; }
        public VehicleState State { get; set; }
        public Vertex<VertexInfo, EdgeInfo> CurrentVertexPosition { get; set; } 
        public Vertex<VertexInfo, EdgeInfo> Target { get; set; }
        public CompletedOrder CurrentOrder { get; set; }

        public List<Vertex<VertexInfo, EdgeInfo>> PathToTarget { get; private set; }
        public double DistanceToTarget { get; private set; }
        public double DistanceTraveled { get; private set; }

        public void AssignOrder(Order order)
        {
            CurrentOrder = new CompletedOrder(order);
            Target = order.Target;
            State = VehicleState.MovingToTarget;
            DistanceTraveled = 0d;
            Simulation.OpenOrders.Remove(order);

            var (path, distance) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, order.Start, order.Target);
            PathToTarget = path;
            DistanceToTarget = distance;
        }

        public void AssignOrderAtDifferentBase(Order order, Vertex<VertexInfo, EdgeInfo> target)
        {
            CurrentOrder = new CompletedOrder(order);
            Target = target;
            State = VehicleState.PickingUpOrder;
            DistanceTraveled = 0d;
            Simulation.OpenOrders.Remove(order);

            var (path, distance) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, Target);
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
            else if (State == VehicleState.PickingUpOrder)
            {
                if (Target != null)
                {
                    DistanceTraveled += (Speed / 3.6d) * Simulation.SecondsPerSimulationStep;

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier,
                            $"Moving vehicle { GetHashCode() } to base { Target.Info.Name } from { CurrentVertexPosition.Info.Name } to pick up next order ({ DistanceTraveled / DistanceToTarget * 100d:n2}%)"));

                    // Arrived at base station
                    if (DistanceTraveled >= DistanceToTarget)
                    {
                        State = VehicleState.MovingToTarget;
                        CurrentVertexPosition = Target;
                        DistanceTraveled = 0d;
                    }
                }
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
                        State = VehicleState.Idle;
                    }
                }
            }  
        }
    }
}