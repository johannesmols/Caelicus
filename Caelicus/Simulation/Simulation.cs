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
using Newtonsoft.Json;

namespace Caelicus.Simulation
{
    public class Simulation
    {
        public IProgress<SimulationProgress> ProgressReporter { get; private set; }
        public readonly SimulationParameters Parameters;
        public List<VehicleInstance> Vehicles { get; } = new List<VehicleInstance>();
        public List<Order> OpenOrders { get; } = new List<Order>();
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

                Vehicles.Add(new VehicleInstance(this, Parameters.VehicleTemplate, allBases[new Random(Parameters.RandomSeed + i).Next(allBases.Count)]));
                currentBaseIndex++;
            }

            // Generate random orders
            for (var i = 0; i < Parameters.NumberOfOrders; i++)
            {
                // TODO: Generate semi-random payload weight
                OpenOrders.Add(new Order(allBases[new Random(Parameters.RandomSeed + i).Next(allBases.Count)], allTargets[new Random(Parameters.RandomSeed + i).Next(allTargets.Count)], 10));
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
            ProgressReporter = progress;
            ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier, $"Starting simulation with  { Parameters.NumberOfVehicles } { Parameters.VehicleTemplate.Name }"));

            while (!IsDone())
            {
                ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier, 
                    $"Simulating at step { SimulationStep }: " +
                    $"({ OpenOrders.Count } open orders, { ClosedOrders.Count } closed orders, { Vehicles.Where(v => v.CurrentOrder != null).ToList().Count } orders in progress)"));

                Advance();

                // Wait for an amount of time corresponding to the simulation speed (e.g. speed of 1 = 1 step per second, speed of 2 = 2 steps per second, ...)
                await Task.Delay((int) (SecondsPerSimulationStep * 1000));

                // Use this snippet to repeatedly check for cancellation in each iteration of the simulation
                if (cancellationToken.IsCancellationRequested)
                {
                    ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier, $"Stopped simulation with { Parameters.NumberOfVehicles } { Parameters.VehicleTemplate.Name }"));
                    throw new TaskCanceledException();
                }
            }

            ProgressReporter.Report(new SimulationProgress(Parameters.SimulationIdentifier, $"Finished simulation with { Parameters.NumberOfVehicles } { Parameters.VehicleTemplate.Name }"));

            // TODO: Return actual results
            return new SimulationResult(Parameters.SimulationIdentifier, "Success");
        }

        /// <summary>
        /// Determines whether all orders have been fulfilled successfully
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            return OpenOrders.Count == 0 && Vehicles.All(v => v.CurrentOrder == null);
        }

        /// <summary>
        /// Advance the simulation by a single step
        /// </summary>
        public void Advance()
        {
            // Assign open orders to any available vehicle
            foreach (var vehicle in Vehicles.Where(vehicle => vehicle.State == VehicleState.Idle))
            {
                if (OpenOrders.Where(o => o.Start == vehicle.CurrentVertexPosition).ToList().Count > 0)
                {
                    var order = OpenOrders.First(o => o.Start == vehicle.CurrentVertexPosition);
                    vehicle.AssignOrder(order);
                    OpenOrders.Remove(order);
                }
                else
                {
                    // TODO Move vehicle to another base where there is an order available
                }
            }

            // Advance all vehicles and their assigned orders
            Vehicles.ForEach(v => v.Advance());

            SimulationStep++;
        }
    }
}
