using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Graph;
using Caelicus.Models.Graph;

namespace Caelicus.Simulation
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
    }
}
