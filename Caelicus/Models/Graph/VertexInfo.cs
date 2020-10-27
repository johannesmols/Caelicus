using System;
using Caelicus.Enums;

namespace Caelicus.Models.Graph
{
    public class VertexInfo
    {
        public string Name { get; set; }
        public VertexType Type { get; set; }
        public Tuple<double, double> Position { get; set; }
    }
}
