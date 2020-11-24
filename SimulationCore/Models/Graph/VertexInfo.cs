using System;
using SimulationCore.Enums;

namespace SimulationCore.Models.Graph
{
    public class VertexInfo
    {
        private const double Tolerance = 0.0010;
        public string Name { get; set; }
        public VertexType Type { get; set; }
        public Tuple<double, double> Position { get; set; }

        public bool Equals(VertexInfo obj)
        {
            return Name == obj.Name && obj.Type == Type && (Math.Abs(Position.Item1 - obj.Position.Item1) < Tolerance &&
                    Math.Abs(Position.Item2 - obj.Position.Item2) < Tolerance);
        }
    }
}
