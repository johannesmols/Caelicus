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
            
        }

        public double DeliveryTime { get; set; }

        public double DeliveryDistance { get; set; }

        public List<Vertex<VertexInfo, EdgeInfo>> DeliveryPath { get; set; }
    }
}
