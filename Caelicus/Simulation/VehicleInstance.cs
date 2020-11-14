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
            CurrentFuelLoaded = FuelCapacity;
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
                    MoveTowardsPickup();
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
                    orders.ForEach(o => Simulation.OpenOrders.Remove(o.Order));
                    PathToTarget = orders.First().DeliveryPath;
                    State = VehicleState.MovingToTarget;
                    DistanceTraveled = 0d;
                }
                else
                {
                    CurrentOrders = orders;
                    orders.ForEach(o => Simulation.OpenOrders.Remove(o.Order));
                    PathToTarget = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, orders.First().Start).Item1;
                    State = VehicleState.PickingUpOrder;
                    DistanceTraveled = 0d;
                }
            }
        }

        private void MoveTowardsTarget()
        {
            if (Move())
            {
                // Finish orders
                Simulation.ClosedOrders.AddRange(CurrentOrders);

                // Clear order-related variables
                CurrentOrders.Clear();
                PathToTarget.Clear();
                CurrentTarget = null;
                DistanceToCurrentTarget = 0d;
                DistanceTraveled = 0d;

                // Change back to idle to receive new orders
                State = VehicleState.Idle;
            }
        }

        private void MoveTowardsPickup()
        {
            if (Move())
            {
                PathToTarget = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentOrders.First().Start, CurrentOrders.First().Target).Item1;
                DistanceToCurrentTarget = 0d;
                DistanceTraveled = 0d;

                // Change state to indicate the vehicle is now delivering the orders
                State = VehicleState.MovingToTarget;
            }
        }

        /// <summary>
        /// Move along the path towards the specified target
        /// <returns>Whether it arrived at the final target</returns>
        /// </summary>
        private bool Move()
        {
            if (CurrentTarget == null && PathToTarget.Count >= 2)
            {
                CurrentTarget = PathToTarget[1];
                DistanceToCurrentTarget = PathToTarget[0].Edges.First(e => e.Destination == PathToTarget[1]).Info.Distance;
            }

            if (DistanceTraveled >= DistanceToCurrentTarget)
            {
                CurrentVertexPosition = CurrentTarget;

                var currentIndexInPath = PathToTarget.IndexOf(CurrentVertexPosition);
                if (currentIndexInPath == PathToTarget.Count - 1)
                {
                    return true;
                }

                CurrentTarget = PathToTarget[currentIndexInPath + 1];
                DistanceTraveled = 0d;
                DistanceToCurrentTarget = PathToTarget[currentIndexInPath].Edges.First(e => e.Destination == PathToTarget[currentIndexInPath + 1]).Info.Distance;
            }
            else
            {
                DistanceTraveled += GetSpeedInMetersPerSecond();
                CurrentFuelLoaded -= GetFuelConsumptionForOneMeter(CurrentOrders.Sum(o => o.Order.PayloadWeight));
            }

            return false;
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
    }
}