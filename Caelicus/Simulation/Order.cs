using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
     public enum OrderStatus
     {
        Done,
        Pending,
        Active,
        Enqueued
     }

    public class Order
    {
        
        public VertexInfo Target { get; }
        
        public int StartTime { get; set; }

        public VehicleInstance AssignedVehicle { get; set; }
        public OrderStatus Status { get; set; }

        public bool IsActive()
        {
            return Status == OrderStatus.Active;
        }
        public bool IsPending()
        {
            return Status == OrderStatus.Pending;
        }
        public bool IsDone()
        {
            return Status == OrderStatus.Done;
        }
        
        public bool IsEnqueue()
        {
            return Status == OrderStatus.Enqueued;
        }

        public void SetVehicle(VehicleInstance v)
        {
            Status = OrderStatus.Active;
            AssignedVehicle = v;
        }
        
        public Order(int startTime, VertexInfo target)
        {
            Status = OrderStatus.Enqueued;
            StartTime = startTime;
            Target = target;
        }
    }
}