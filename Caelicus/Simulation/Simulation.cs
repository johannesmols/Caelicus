using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public class Simulation
    {
        private readonly SimulationParameters _parameters;

        public Simulation(SimulationParameters parameters)
        {
            _parameters = parameters;
        }

        public async Task<SimulationResult> Simulate(IProgress<SimulationProgress> progress, CancellationToken cancellationToken)
        {
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Starting simulation with  { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));
            var simulator = new Simulator(_parameters);
            while (!simulator.IsDone())
            {
                simulator.advance();
                // TODO: Perform the actual simulation
                await Task.Delay((int) (_parameters.SimulationDuration / _parameters.SimulationSpeed));

                // Use this snippet to repeatedly check for cancellation in each iteration of the simulation
                if (cancellationToken.IsCancellationRequested)
                {
                progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Stopped simulation with { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));
                    throw new TaskCanceledException();
                }

            }
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Finished simulation with { _parameters.NumberOfVehicles } { _parameters.VehicleTemplate.Name }"));

            return new SimulationResult(_parameters.SimulationIdentifier, "Success");
        }
    }
    
    public class RandomPenaltiesGenerator
    {
        private Random generator = new Random();
        private List<VehicleInstance> _vehicles;
        public RandomPenaltiesGenerator(List<VehicleInstance> v)
        {
            _vehicles = v;
        }

        public void GenerateRandomness()
        {
            foreach (var v in _vehicles.Where(v => generator.Next() % 2 == 0))
            {
                v.MovementPenalty = generator.NextDouble();
            }
        }
    }

    public class Simulator
    {
        private SimulationParameters _parameters;
        private VehicleController _controller;
        private RandomPenaltiesGenerator _generator;
        private int _time = 0;

        public bool IsDone()
        {
            return _parameters.Missions.All(o => o.IsDone());
        }
        public Simulator(SimulationParameters p)
        {
            _time = 0; 
            _parameters = p;
            _controller = new VehicleController(_parameters.Vehicles, _parameters.Missions, _parameters.Graph);
            _generator = new RandomPenaltiesGenerator(_parameters.Vehicles);
        }

        public void reset()
        {
            var bases = _parameters.Graph.Where(vertex => vertex.Info.Type == VertexType.Base).Select(vertex => vertex.Info).ToList();
            for (int i = 0; i < _parameters.Vehicles.Count; i++)
            {
                _parameters.Vehicles[i].State = VehicleState.Steady;
                _parameters.Vehicles[i].Base = bases[bases.Count % i];
            }
            _time = 0;
        }

        public bool advance()
        {
            _time += 1;
            _controller.CheckForCompletedOrders();
            _controller.CheckVehicleToBase();
            _controller.UpdatePendingOrders();
            _controller.SetupVehicles();
            _generator.GenerateRandomness();
            _controller.advanceTime();
            return true;
        }
    }

    
    public class VehicleController
    {
        private List<VehicleInstance> _vehicles;
        private List<Order> _missions;
        private int Time { get; set; }

        public Graph<VertexInfo, EdgeInfo> _graph { get; set; }
        
        public VehicleController(List<VehicleInstance> vehicles, List<Order> missions, Graph<VertexInfo, EdgeInfo> g)
        {
            _graph = g;
            _vehicles = vehicles;
            _missions = missions;
            Time = 0;
        }

        public void CheckForCompletedOrders()
        {
            foreach (var m in _missions.Where(m => m.IsActive()).Where(m => m.AssignedVehicle.ArrivedToTarget()))
            {
                m.Status = OrderStatus.Done;
                m.AssignedVehicle.ReturnToBase();
                m.AssignedVehicle.SetOrder(null);
            }
        }
        
        public void CheckVehicleToBase()
        {
            foreach (var v in _vehicles.Where(v => v.State == VehicleState.MovingToBase))
            {
                if (v.ArrivedToTarget())
                {
                    v.State = VehicleState.Steady;
                }
            }
        }
        
        /// <summary>
        /// Check if there are missions to start and set them to Pending status
        /// </summary>
        public void UpdatePendingOrders()
        {
            foreach (var m in _missions.Where(m => m.IsEnqueue()))
            {
                if (m.StartTime == Time)
                {
                    m.Status = OrderStatus.Pending;
                }
            }
        }

        public void advanceTime()
        {
            Time += 1;
            foreach (var v in _vehicles.Where(v => v.IsMoving()))
            {
                v.Advance();
            }
        }
/// <summary>
/// TODO: complete the search
/// </summary>
/// <param name="t"></param>
/// <param name="b"></param>
/// <returns></returns>
        float CalculateDistance(VertexInfo t, VertexInfo b)
        {
            float distance = _graph.DepthFirstFind(t, b);
            return distance;
        }

        public void SetupVehicles()
        {
            foreach (var v in _vehicles.Where(v => v.State == VehicleState.Steady))
            {
                foreach (var m in _missions.Where(m => m.IsPending()))
                {
                    v.SetOrder(m.Target);
                    m.SetVehicle(v);
                    v.Distance = CalculateDistance(m.Target, v.Base);
                }
            }
        }
    }
}
