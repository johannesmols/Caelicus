using System.Collections.Generic;
using SimulationCore.Graph;
using SimulationCore.Models.Graph;

namespace SimulationCore.Simulation
{
    public class CompletedOrder : Order
    {
        public CompletedOrder(Order order) : base(order)
        {
            Order = order;
        }

        public Order Order { get; }

        public List<Vertex<VertexInfo, EdgeInfo>> DeliveryPath { get; set; }


        // Statistics about order delivery
        public double DeliveryTime { get; set; }

        public double DeliveryDistance { get; set; }

        public double DeliveryCost { get; set; }
    }
}
