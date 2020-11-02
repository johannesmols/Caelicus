using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Starting simulation with  { _parameters.Vehicles.FirstOrDefault()?.Item2 } vehicles"));
            int currentTime = 0;
            while (currentTime < _parameters.SimulationDuration)
            {
                // TODO: Perform the actual simulation
                await Task.Delay((int) (_parameters.SimulationDuration / _parameters.SimulationSpeed));

                // Use this snippet to repeatedly check for cancellation in each iteration of the simulation
                if (cancellationToken.IsCancellationRequested)
                {
                    progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Stopped simulation with { _parameters.Vehicles.FirstOrDefault()?.Item2 } vehicles"));
                    throw new TaskCanceledException();
                }

            }
            progress.Report(new SimulationProgress(_parameters.SimulationIdentifier, $"Finished simulation with { _parameters.Vehicles.FirstOrDefault()?.Item2 } vehicles"));

            return new SimulationResult(_parameters.SimulationIdentifier, "Success");
        }
    }
    
    public class RandomPenaltiesGenerator
    {
        private Random generator = new Random();
        private List<Vehicle> _vehicles;
        public RandomPenaltiesGenerator(List<Vehicle> v)
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
        public Simulator(SimulationParameters p)
        {
            _time = 0; 
            _parameters = p;
            _controller = new VehicleController(_parameters.Vehicles.Select(x => x.Item1).ToList(), _parameters.Missions);
            _generator = new RandomPenaltiesGenerator(_parameters.Vehicles.Select(x => x.Item1).ToList());
        }

        public void reset()
        {
            _time = 0;
        }

        public bool advance()
        {
            _time += 1;
            _controller.CheckForCompletedMissions();
            _controller.UpdatePendingMissions();
            _controller.SetupVehicles();
            _generator.GenerateRandomness();
            _controller.advanceTime();
            return true;
        }
    }

    
    public class VehicleController
    {
        private List<Vehicle> _vehicles;
        private List<Mission> _missions;
        private int Time { get; set; }

        public VehicleController(List<Vehicle> vehicles, List<Mission> missions)
        {
            _vehicles = vehicles;
            _missions = missions;
            Time = 0;
        }

        public void CheckForCompletedMissions()
        {
            foreach (var m in _missions.Where(m => m.IsActive()).Where(m => m.AssignedVehicle.ArrivedToTarget()))
            {
                m.Status = MissionStatus.Done;
                m.AssignedVehicle.ReturnToBase();
            }
        }
        
        /// <summary>
        /// Check if there are missions to start and set them to Pending status
        /// </summary>
        public void UpdatePendingMissions()
        {
            foreach (var m in _missions.Where(m => m.IsEnqueue()))
            {
                if (m.StartTime == Time)
                {
                    m.Status = MissionStatus.Pending;
                }
            }
        }

        public void advanceTime()
        {
            Time += 1;
            foreach (var v in _vehicles.Where(v => v.State == VehicleState.Moving))
            {
                v.Advance();
            }
        }

        public void SetupVehicles()
        {
            foreach (var v in _vehicles.Where(v => v.State == VehicleState.Steady))
            {
                foreach (var m in _missions.Where(m => m.IsPending()))
                {
                    v.SetMission(m.Target);
                    m.SetVehicle(v);
                }
            }
        }
    }
}
