using Caelicus.Graph;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public class Order
    {
        public Order(Vertex<VertexInfo, EdgeInfo> start, Vertex<VertexInfo, EdgeInfo> target, double payloadWeight)
        {
            Start = start;
            Target = target;
            PayloadWeight = payloadWeight;
        }

        public Order(Order order)
        {
            Start = order.Start;
            Target = order.Target;
            PayloadWeight = order.PayloadWeight;
        }

        public readonly Vertex<VertexInfo, EdgeInfo> Start;
        public readonly Vertex<VertexInfo, EdgeInfo> Target;
        public readonly double PayloadWeight;
    }
}