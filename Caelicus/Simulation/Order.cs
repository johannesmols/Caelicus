using Caelicus.Graph;
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
        public Order(Vertex<VertexInfo, EdgeInfo> start, Vertex<VertexInfo, EdgeInfo> target)
        {
            Status = OrderStatus.Enqueued;
            Start = start;
            Target = target;
        }

        public Vertex<VertexInfo, EdgeInfo> Start { get; }

        public Vertex<VertexInfo, EdgeInfo> Target { get; set; }


        // Runtime variables

        public OrderStatus Status { get; set; }

        public VehicleInstance AssignedVehicle { get; private set; }

        public void AssignVehicle(VehicleInstance vehicle)
        {
            Status = OrderStatus.Active;
            AssignedVehicle = vehicle;
        }
    }
}