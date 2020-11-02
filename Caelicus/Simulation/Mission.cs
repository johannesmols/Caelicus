using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
     public enum MissionStatus
     {
        Done,
        Pending,
        Active,
        Enqueued
     }

    public class Mission
    {
        
        private VertexInfo _target;
        private float _startTime;
        public Vehicle AssignedVehicle { get; set; }
        public MissionStatus Status { get; set; }

        public bool IsActive()
        {
            return Status == MissionStatus.Active;
        }

        public Mission(float startTime, VertexInfo target)
        {
            Status = MissionStatus.Enqueued;
            _startTime = startTime;
            _target = target;
        }
    }
}