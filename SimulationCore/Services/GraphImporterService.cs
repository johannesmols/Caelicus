using System;
using System.Linq;
using GoogleMapsComponents.Maps;
using SimulationCore.Enums;
using SimulationCore.Graph;
using SimulationCore.Helpers;
using SimulationCore.Models.Graph;

namespace SimulationCore.Services
{
    public static class GraphImporterService
    {
        public static Graph<VertexInfo, EdgeInfo> GenerateGraph(JsonGraphRootObject json)
        {
            var graph = new Graph<VertexInfo, EdgeInfo>();

            // Add vertices
            foreach (var vertex in json.Vertices)
            {
                graph.AddVertex(new VertexInfo()
                {
                    Name = vertex.Name,
                    Type = DetermineVertexType(vertex.Type),
                    Position = new Tuple<double, double>(vertex.Latitude, vertex.Longitude)
                });
            }

            // Add edges
            foreach (var vertex in json.Vertices)
            {
                foreach (var edge in vertex.Edges)
                {
                    var origin = graph.CustomFirstOrDefault(v => v.Name == vertex.Name);
                    var destination = graph.CustomFirstOrDefault(v => v.Name == edge.Target);

                    if (origin != null && destination != null)
                    {
                        graph.AddEdge(origin, destination, new EdgeInfo()
                        {
                            Distance = GeographicalHelpers.CalculateGeographicalDistanceInMeters(origin.Info.Position, destination.Info.Position),
                            GMapsDistanceAndTime = edge.Modes.ToDictionary(mode => Enum.Parse<TravelMode>(mode.TravelMode), mode => Tuple.Create((double)mode.Distance, (double)mode.Time))
                        });
                    }
                }
            }

            return graph;
        }

        private static VertexType DetermineVertexType(string type)
        {
            try
            {
                return Enum.Parse<VertexType>(type, true);
            }
            catch
            {
                Console.WriteLine($"Error while parsing graph JSON for a vertex type " +
                                  $"(type given was { type } but only { VertexType.Base }, " +
                                  $"{ VertexType.Target } and { VertexType.Both } are allowed)");
                
                return VertexType.Target;
            }
        }
    }
}
