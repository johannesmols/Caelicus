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

    public abstract class Event
    {
        private int id;
    }

    public class Simulator
    {
        private SimulationParameters _parameters;
        private VehicleController _controller;
        public Simulator(SimulationParameters p)
        {
            
            _parameters = p;
            _controller = new VehicleController(_parameters.Vehicles.Select(x => x.Item1).ToList(), _parameters.Missions);
        }

        public bool advance()
        {
            return true;
        }
    }

    
    public class VehicleController
    {
        private List<Vehicle> _vehicles;
        private List<Event> _events;
        private List<Mission> _missions;

        public VehicleController(List<Vehicle> vehicles, List<Mission> missions)
        {
            _vehicles = vehicles;
            _missions = missions;
            _events = new List<Event>();
        }

        public void EnqueueEvent(Event e)
        {
            _events.Add(e);
        }

        public void CheckForCompletedMissions()
        {
            foreach (var m in _missions.Where(m => m.IsActive()).Where(m => m.AssignedVehicle.ArrivedToTarget()))
            {
                m.Status = MissionStatus.Done;
            }
        }
        
    }
}
