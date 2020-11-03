using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Helpers;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;
using GeoCoordinatePortable;

namespace Caelicus.Simulation
{
    public class Simulation
    {
        private readonly Random _random;

        private readonly SimulationParameters _parameters;
        private List<VehicleInstance> _vehicles = new List<VehicleInstance>();
        private List<Order> _orders = new List<Order>();

        private readonly double _secondsPerSimulationStep;
        private int _simulationStep = 0;

        /// <summary>
        /// Generates list of vehicles and orders based on the simulation parameters
        /// </summary>
        /// <param name="parameters">simulation parameters</param>
        public Simulation(SimulationParameters parameters)
        {
            _parameters = parameters;
            _random = new Random(_parameters.RandomSeed);
            _secondsPerSimulationStep = 1d / _parameters.SimulationSpeed;

            var allBases = _parameters.Graph.Vertices.Where(v => v.Info.Type == VertexType.Base).ToList();
            var allTargets = _parameters.Graph.Vertices.Where(v => v.Info.Type == VertexType.Target).ToList();

            // Equally split vehicles up to base stations
            var currentBaseIndex = 0;
            for (var i = 0; i < _parameters.NumberOfVehicles; i++)
            {
                if (currentBaseIndex == allBases.Count)
                {
                    currentBaseIndex = 0;
                }

                _vehicles.Add(new VehicleInstance(_parameters.VehicleTemplate, allBases[_random.Next(allBases.Count - 1)]));
                currentBaseIndex++;
            }

            // Generate random orders
            for (var i = 0; i < _parameters.NumberOfOrders; i++)
            {
                _orders.Add(new Order(allBases[_random.Next(allBases.Count - 1)], allTargets[_random.Next(allTargets.Count - 1)]));
            }
        }

        /// <summary>
        /// Main thread of running the simulation, posting updates, and finally returning the results
        /// </summary>
        /// <param name="progress">Can be used to send status updates back to the UI</param>
        /// <param name="cancellationToken">Can be used to cancel the operation from the UI</param>
        /// <returns></returns>
        public async Task<SimulationResult> Simulate(IProgress<SimulationProgress> progress, CancellationToken cancellationToken)
        {
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Starting simulation with  { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));

            while (!IsDone())
            {
                Advance();

                // Wait for an amount of time corresponding to the simulation speed (e.g. speed of 1 = 1 step per second, speed of 2 = 2 steps per second, ...)
                await Task.Delay((int) (_secondsPerSimulationStep * 1000), cancellationToken);

                // Use this snippet to repeatedly check for cancellation in each iteration of the simulation
                if (cancellationToken.IsCancellationRequested)
                {
                    progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Stopped simulation with { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));
                    throw new TaskCanceledException();
                }
            }

            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Finished simulation with { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));

            // TODO: Return actual results
            return new SimulationResult(_parameters.SimulationIdentifier, "Success");
        }

        /// <summary>
        /// Determines whether all orders have been fulfilled successfully
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            return _orders.All(o => o.Status == OrderStatus.Done);
        }

        /// <summary>
        /// Advance the simulation by a single step
        /// </summary>
        public void Advance()
        {
            _simulationStep++;

            CheckForCompletedOrders();
            CheckVehicleToBase();
            UpdatePendingOrders();
            SetupVehicles();
            AdvanceVehicles();
        }

        /// <summary>
        /// Check if any orders have been completed in the previous step and mark them as done
        /// </summary>
        private void CheckForCompletedOrders()
        {
            foreach (var order in _orders.Where(o => o.Status == OrderStatus.Active).Where(o => o.AssignedVehicle.ArrivedAtTarget()))
            {
                order.Status = OrderStatus.Done;
                order.AssignedVehicle.ReturnToBase();
                order.AssignedVehicle.StartOrder(null);
            }
        }

        private void CheckVehicleToBase()
        {
            foreach (var vehicle in _vehicles.Where(v => v.State == VehicleState.MovingToBase))
            {
                if (vehicle.ArrivedAtTarget())
                {
                    vehicle.State = VehicleState.Idle;
                }
            }
        }

        private void UpdatePendingOrders()
        {
            foreach (var order in _orders.Where(m => m.Status == OrderStatus.Enqueued))
            {
                // TODO Create logic to determine when an order should be started (e.g. whether there is a vehicle available at the start base)
                order.Status = OrderStatus.Pending;
            }
        }

        private void SetupVehicles()
        {
            foreach (var vehicle in _vehicles.Where(v => v.State == VehicleState.Idle))
            {
                foreach (var order in _orders.Where(m => m.Status == OrderStatus.Pending))
                {
                    vehicle.StartOrder(order.Target);
                    order.AssignVehicle(vehicle);

                    // TODO Calculate actual distance from the target using path-finding algorithm minus the already traveled distance
                    vehicle.Distance = GeographicalHelpers.CalculateGeographicalDistanceInMeters(order.Target.Info.Position, vehicle.CurrentVertexPosition.Info.Position);
                }
            }
        }

        private void AdvanceVehicles()
        {
            foreach (var vehicle in _vehicles.Where(v => v.IsMoving()))
            {
                vehicle.Advance();
            }
        }
    }
}
