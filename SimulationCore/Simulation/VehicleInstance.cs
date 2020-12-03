using System;
using System.Collections.Generic;
using System.Linq;
using SimulationCore.Enums;
using SimulationCore.Graph;
using SimulationCore.Models.Graph;
using SimulationCore.Models.Vehicles;

namespace SimulationCore.Simulation
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
        public double TimeToCurrentTarget { get; private set; }
        public double DistanceTraveled { get; private set; }

        // Statistics
        public double TotalTravelDistance { get; private set; }
        public double TotalTravelTime { get; private set; }
        public double TotalIdleTime { get; private set; }

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
                    TotalIdleTime++;
                    break;
                case VehicleState.Refueling:
                    Refuel();
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
            if (orders?.Count > 0)
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
                    PathToTarget = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, orders.First().Start, TravelMode).Item1;
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
                TimeToCurrentTarget = 0d;
                DistanceTraveled = 0d;

                State = VehicleState.Idle;
            }
        }

        private void MoveTowardsPickup()
        {
            if (Move())
            {
                PathToTarget = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentOrders.First().Start, CurrentOrders.First().Target, TravelMode).Item1;
                DistanceToCurrentTarget = 0d;
                TimeToCurrentTarget = 0d;
                DistanceTraveled = 0d;

                // Refuel the vehicle before heading out to deliver
                State = VehicleState.Refueling;
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
                DistanceToCurrentTarget = Vehicle.TravelMode == GoogleMapsComponents.Maps.TravelMode.Transit ?
                    PathToTarget[0].Edges.First(e => e.Destination == PathToTarget[1]).Info.Distance :
                    PathToTarget[0].Edges.First(e => e.Destination == PathToTarget[1]).Info.GMapsDistanceAndTime[Vehicle.TravelMode].Item1;
                TimeToCurrentTarget = Vehicle.TravelMode == GoogleMapsComponents.Maps.TravelMode.Transit ? 0d
                    : PathToTarget[0].Edges.First(e => e.Destination == PathToTarget[1]).Info.GMapsDistanceAndTime[Vehicle.TravelMode].Item2;
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
                DistanceToCurrentTarget = Vehicle.TravelMode == GoogleMapsComponents.Maps.TravelMode.Transit ?
                    PathToTarget[currentIndexInPath].Edges.First(e => e.Destination == PathToTarget[currentIndexInPath + 1]).Info.Distance :
                    PathToTarget[currentIndexInPath].Edges.First(e => e.Destination == PathToTarget[currentIndexInPath + 1]).Info.GMapsDistanceAndTime[Vehicle.TravelMode].Item1;
                TimeToCurrentTarget = Vehicle.TravelMode == GoogleMapsComponents.Maps.TravelMode.Transit ? 0d
                    : PathToTarget[currentIndexInPath].Edges.First(e => e.Destination == PathToTarget[currentIndexInPath + 1]).Info.GMapsDistanceAndTime[Vehicle.TravelMode].Item2;
            }
            else
            {
                DistanceTraveled += GetSpeedInMetersPerSecond(DistanceToCurrentTarget, TimeToCurrentTarget);

                if (State == VehicleState.MovingToTarget)
                {
                    CurrentFuelLoaded -= GetFuelConsumptionForOneMeter(CurrentOrders?.Sum(o => o.Order.PayloadWeight) ?? 0d) * GetSpeedInMetersPerSecond(DistanceToCurrentTarget, TimeToCurrentTarget);
                }
                else if (State == VehicleState.PickingUpOrder)
                {
                    CurrentFuelLoaded -= BaseFuelConsumption * GetSpeedInMetersPerSecond(DistanceToCurrentTarget, TimeToCurrentTarget);
                }

                // Record progress in order
                CurrentOrders.ForEach(o =>
                {
                    if (State == VehicleState.MovingToTarget)
                    {
                        o.DeliveryTime++;
                        o.DeliveryDistance += GetSpeedInMetersPerSecond(DistanceToCurrentTarget, TimeToCurrentTarget);
                        o.DeliveryCost = CalculateJourneyCost(o.DeliveryDistance, o.DeliveryTime) / CurrentOrders.Count; // divide cost depending how many orders are loaded
                    }
                    else if (State == VehicleState.PickingUpOrder)
                    {
                        o.PickupTime++;
                        o.PickupDistance += GetSpeedInMetersPerSecond(DistanceToCurrentTarget, TimeToCurrentTarget);
                        o.PickupCost = CalculateJourneyCost(o.PickupDistance, o.PickupTime) / CurrentOrders.Count; // divide cost depending how many orders are loaded
                    }
                });

                // Record statistics
                TotalTravelDistance += GetSpeedInMetersPerSecond(DistanceToCurrentTarget, TimeToCurrentTarget);
                TotalTravelTime++;
            }

            return false;
        }

        /// <summary>
        /// Refuel the vehicle
        /// </summary>
        private void Refuel()
        {
            CurrentFuelLoaded += (FuelCapacity / RefuelingTime);

            if (CurrentFuelLoaded > FuelCapacity)
            {
                CurrentFuelLoaded = FuelCapacity;
                State = VehicleState.MovingToTarget;
            }
        }

        /// <summary>
        /// Find the nearest available orders that this vehicle can fulfill
        /// </summary>
        public List<CompletedOrder> FindOptimalOrders()
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
                    .OrderBy(o => Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, o.Key, TravelMode).Item2)
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
                    .OrderBy(o => Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, start, o.Key, TravelMode).Item2)
                    .FirstOrDefault();

                var payloadSoFar = 0d;

                if (ordersSortedByNearestTarget != null)
                {
                    foreach (var order in ordersSortedByNearestTarget.OrderBy(o => o.PayloadWeight))
                    {
                        var (path, distance, time) = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, order.Start, order.Target, TravelMode);

                        // Add distance between current position and to start, if any
                        var distanceToPickup = 0d;
                        if (order.Start != CurrentVertexPosition)
                        {
                            distanceToPickup = Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, CurrentVertexPosition, order.Start, TravelMode).Item2;
                        }

                        // Add distance to nearest base station after target to check whether the vehicle can still get back to refuel
                        var distanceToNextBaseStation = 0d;
                        if (!AllowRefuelAtTarget)
                        {
                            distanceToNextBaseStation = Simulation.Parameters.Graph.Vertices
                                .Where(v => v.Info.Type == VertexType.Base)
                                .Select(b => Simulation.Parameters.Graph.FindShortestPath(Simulation.Parameters.Graph, order.Target, b))
                                .OrderBy(d => d.Item2).First().Item2;
                        }

                        if (GetMaximumTravelDistance(0d, CurrentFuelLoaded) >= distanceToPickup && // do i have enough fuel to go pick up the order?
                            GetMaximumTravelDistance(payloadSoFar + order.PayloadWeight) >= distance && // do i have enough fuel to deliver the order with a full fuel tank + payload?
                            GetMaximumTravelDistance(0d, FuelCapacity - GetFuelConsumptionForOneMeter(payloadSoFar + order.PayloadWeight) * distance) >= distanceToNextBaseStation && // do i have enough fuel to get to the nearest base station after delivery?
                            payloadSoFar + order.PayloadWeight <= MaxPayload)
                        {
                            // TODO: For now only allow one order at a time
                            if (selectedOrders.Count <= 1)
                            {
                                selectedOrders.Add(Tuple.Create(order, path));
                                payloadSoFar += order.PayloadWeight;
                            }
                        }
                    }
                }
            }

            return selectedOrders.Select(o => new CompletedOrder(o.Item1)
            {
                DeliveryPath = o.Item2,
                DeliveryTime = 0,
                DeliveryDistance = 0,
                DeliveryCost = 0,
                PickupTime = 0,
                PickupDistance = 0,
                PickupCost = 0
            }).ToList();
        }
    }
}