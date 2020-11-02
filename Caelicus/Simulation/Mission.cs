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
        
        public VertexInfo Target { get; }
        
        public int StartTime { get; set; }

        public Vehicle AssignedVehicle { get; set; }
        public MissionStatus Status { get; set; }

        public bool IsActive()
        {
            return Status == MissionStatus.Active;
        }
        public bool IsPending()
        {
            return Status == MissionStatus.Pending;
        }
        public bool IsDone()
        {
            return Status == MissionStatus.Done;
        }
        
        public bool IsEnqueue()
        {
            return Status == MissionStatus.Enqueued;
        }

        public void SetVehicle(Vehicle v)
        {
            Status = MissionStatus.Active;
            AssignedVehicle = v;
        }
        
        public Mission(int startTime, VertexInfo target)
        {
            Status = MissionStatus.Enqueued;
            StartTime = startTime;
            Target = target;
        }
    }
}