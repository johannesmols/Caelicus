using System;
using System.Collections.Generic;
using System.Linq;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Helpers;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;
using Newtonsoft.Json.Bson;

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

        // State
        public Vehicle Vehicle { get; }
        public VehicleState State { get; private set; }

        // Routing
        public List<Vertex<VertexInfo, EdgeInfo>> PathToTarget { get; private set; }
        public Vertex<VertexInfo, EdgeInfo> CurrentVertexPosition { get; private set; } 
        public Vertex<VertexInfo, EdgeInfo> CurrentTarget { get; private set; }
        public double DistanceToCurrentTarget { get; private set; }
        public double DistanceTraveled { get; private set; }

        // Order management
        public List<CompletedOrder> CurrentOrders { get; private set; }

        // Will be obsolete
        public CompletedOrder CurrentOrder { get; private set; }

        // Fuel
        public double CurrentFuelLoaded { get; private set; }

        // Cost

        public void AdvanceNew()
        {
            switch (State)
            {
                case VehicleState.Idle:
                    AssignOrders(FindOptimalOrders());
                    break;
                case VehicleState.Refueling:
                    break;
                case VehicleState.MovingToTarget:
                    MoveTowardsTarget();
                    break;
                case VehicleState.PickingUpOrder:
                    MoveTowardsTarget();
                    break;
            }
        }

        private void AssignOrders(List<CompletedOrder> orders)
        {
            if (orders != null && orders.Count > 0)
            {
                if (orders.All(o => o.Start == CurrentVertexPosition))
                {
                    CurrentOrders = orders;
                    Simulation.OpenOrders = Simulation.OpenOrders.Except(orders).ToList();
                    PathToTarget = orders.First().DeliveryPath;
                    State = VehicleState.MovingToTarget;
                    DistanceTraveled = 0d;
                }
                else
                {
                    CurrentOrders = orders;
                    Simulation.OpenOrders = Simulation.OpenOrders.Except(orders).ToList();
                    PathToTarget = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, orders.First().Start).Item1;
                    State = VehicleState.PickingUpOrder;
                    DistanceTraveled = 0d;
                }
            }
        }

        private void MoveTowardsTarget()
        {

        }

        private void MoveTowardsPickup()
        {

        }

        private void Move()
        {

        }

        /// <summary>
        /// Find the nearest available orders that this vehicle can fulfill
        /// </summary>
        private List<CompletedOrder> FindOptimalOrders()
        {
            var fulfillableOrders = Simulation.OpenOrders.Where(o => o.PayloadWeight <= MaxPayload).ToList();

            // Orders are available at the current position
            if (fulfillableOrders.Any(o => o.Start == CurrentVertexPosition))
            {
                fulfillableOrders = fulfillableOrders.Where(o => o.Start == CurrentVertexPosition).ToList();
            }
            // Find orders at different positions
            else
            {
                var ordersAtNearestBase = fulfillableOrders
                    .GroupBy(o => o.Start)
                    .OrderBy(o => Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, o.Key).Item2)
                    .FirstOrDefault();

                if (ordersAtNearestBase != null)
                {
                    fulfillableOrders = ordersAtNearestBase.ToList();
                }
            }

            // Select which orders to accept based on target, weight, fuel requirements, etc.
            var selectedOrders = new List<Tuple<Order, List<Vertex<VertexInfo, EdgeInfo>>>>();
            if (fulfillableOrders.Count > 0)
            {
                var start = fulfillableOrders.First().Start;
                var ordersSortedByNearestTarget = fulfillableOrders
                    .GroupBy(o => o.Target)
                    .OrderBy(o => Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, start, o.Key).Item2)
                    .FirstOrDefault();

                var payloadSoFar = 0d;

                if (ordersSortedByNearestTarget != null)
                {
                    foreach (var order in ordersSortedByNearestTarget)
                    {
                        var (path, distance) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, order.Start, order.Target);

                        // Add distance between current position and to start, if any
                        if (order.Start != CurrentVertexPosition)
                        {
                            distance += Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, order.Start).Item2;
                        }

                        if (GetMaximumTravelDistance(payloadSoFar + order.PayloadWeight) >= distance)
                        {
                            selectedOrders.Add(Tuple.Create(order, path));
                            payloadSoFar += order.PayloadWeight;
                        }
                    }
                }
            }

            return selectedOrders.Select(o => new CompletedOrder(o.Item1) { DeliveryPath = o.Item2 }).ToList();
        }


        // Old stuff

        public void AssignOrder(Order order)
        {
            CurrentTarget = order.Target;
            State = VehicleState.MovingToTarget;
            PrepareOrder(order);
        }

        public void AssignOrderAtDifferentBase(Order order, Vertex<VertexInfo, EdgeInfo> target)
        {
            CurrentTarget = target;
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
            DistanceToCurrentTarget = distance;
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
                if (CurrentTarget != null)
                {
                    // Calculate how many meters the vehicle travels in one simulation step
                    // AverageSpeed is in km/h, dividing by 3.6 gives it in m/s.
                    DistanceTraveled += AverageSpeed / 3.6d;

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier,
                            $"Moving vehicle { GetHashCode() } to base { CurrentTarget.Info.Name } from { CurrentVertexPosition.Info.Name } to pick up next order ({ DistanceTraveled / DistanceToCurrentTarget * 100d:n2}%)"));

                    // Arrived at base station
                    if (DistanceTraveled >= DistanceToCurrentTarget)
                    {
                        // Set new position of the vehicle
                        CurrentVertexPosition = CurrentTarget;
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

                    Simulation.ProgressReporter.Report(
                        new SimulationProgress(Simulation.Parameters.SimulationIdentifier, 
                            $"Moving vehicle { GetHashCode() } to target { CurrentOrder.Target.Info.Name } from { CurrentOrder.Start.Info.Name } ({ DistanceTraveled / DistanceToCurrentTarget * 100d:n2}%)"));

                    // Arrived at target
                    if (DistanceTraveled >= DistanceToCurrentTarget)
                    {
                        // Close order
                        Simulation.ClosedOrders.Add(CurrentOrder);
                        CurrentOrder = null;

                        // Set new position of the vehicle
                        CurrentVertexPosition = CurrentTarget;
                        State = VehicleState.Idle;
                    }
                }
            }  
        }
    }
}