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
        Refueling,
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
        public VehicleState State { get; private set; }
        public Vertex<VertexInfo, EdgeInfo> CurrentVertexPosition { get; private set; } 
        public Vertex<VertexInfo, EdgeInfo> Target { get; private set; }
        public CompletedOrder CurrentOrder { get; private set; }

        public List<Vertex<VertexInfo, EdgeInfo>> PathToTarget { get; private set; }
        public double TotalDistanceToTarget { get; private set; }
        public List<Tuple<Vertex<VertexInfo, EdgeInfo>, double>> DistanceToWaypoints { get; private set; }
        public double DistanceTraveled { get; private set; }
        public double CurrentFuel { get; private set; }

        public void AssignOrder(Order order)
        {
            Target = order.Target;
            State = VehicleState.MovingToTarget;
            PrepareOrder(order);
        }

        public void AssignOrderAtDifferentBase(Order order, Vertex<VertexInfo, EdgeInfo> target)
        {
            Target = target;
            State = VehicleState.PickingUpOrder;
            PrepareOrder(order);
        }

        private void PrepareOrder(Order order)
        {
            CurrentOrder = new CompletedOrder(order);
            DistanceTraveled = 0d;
            Simulation.OpenOrders.Remove(order);
            var (path, distance) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, order.Start, order.Target);
            PathToTarget = path;
            TotalDistanceToTarget = distance;
            CurrentOrder.DeliveryDistance = distance;
        }

        public void Advance()
        {
            if (State == VehicleState.Idle)
            {
                Simulation.ProgressReporter.Report(
                    new SimulationProgress(Simulation.Parameters.SimulationIdentifier,
                        $"Vehicle { GetHashCode() } idling at base station { CurrentVertexPosition.Info.Name }"));
            }
            else if (State == VehicleState.Refueling)
            {

            }
            else if (State == VehicleState.PickingUpOrder)
            {
                if (Target != null)
                {
                    // Calculate how many meters the vehicle travels in one simulation step
                    // AverageSpeed is in km/h, dividing by 3.6 gives it in m/s.
                    DistanceTraveled += AverageSpeed / 3.6d;

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier,
                            $"Moving vehicle { GetHashCode() } to base { Target.Info.Name } from { CurrentVertexPosition.Info.Name } to pick up next order ({ DistanceTraveled / TotalDistanceToTarget * 100d:n2}%)"));

                    // Arrived at base station
                    if (DistanceTraveled >= TotalDistanceToTarget)
                    {
                        // Set new position of the vehicle
                        CurrentVertexPosition = Target;
                        AssignOrder(CurrentOrder.Order);
                    }
                }
            }
            else if (State == VehicleState.MovingToTarget)
            {
                if (CurrentOrder != null)
                {
                    // Calculate how many meters the vehicle travels in one simulation step
                    // AverageSpeed is in km/h, dividing by 3.6 gives it in m/s.
                    DistanceTraveled += AverageSpeed / 3.6d;
                    //each step is 1 second
                    CurrentOrder.DeliveryTime++;

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier, 
                            $"Moving vehicle { GetHashCode() } to target { CurrentOrder.Target.Info.Name } from { CurrentOrder.Start.Info.Name } ({ DistanceTraveled / TotalDistanceToTarget * 100d:n2}%)"));

                    // Arrived at target
                    if (DistanceTraveled >= TotalDistanceToTarget)
                    {
                        // Close order
                        Simulation.ClosedOrders.Add(CurrentOrder);
                        CurrentOrder = null;

                        // Set new position of the vehicle
                        CurrentVertexPosition = Target;
                        State = VehicleState.Idle;
                    }
                }
            }  
        }
    }
}