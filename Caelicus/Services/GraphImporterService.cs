using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Models;
using GeoCoordinatePortable;

namespace Caelicus.Services
{
    public class GraphImporterService
    {
        public Graph<VertexInfo, EdgeInfo> GenerateGraph(GraphRootObjectJson json)
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
                    var origin = graph.FirstOrDefault(v => v.Name == vertex.Name);
                    var destination = graph.FirstOrDefault(v => v.Name == edge);

                    if (origin != null && destination != null)
                    {
                        graph.AddEdge(origin, destination, new EdgeInfo() { Distance = CalculateGeographicalDistanceInMeters(origin.Info.Position, destination.Info.Position) });
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
            catch (Exception)
            {
                Console.WriteLine($"Error while parsing graph JSON for a vertex type " +
                                  $"(type given was { type } but only { VertexType.Base } and " +
                                  $"{ VertexType.Target } are allowed)");
                
                return VertexType.Target;
            }
        }

        private static double CalculateGeographicalDistanceInMeters(Tuple<double, double> start, Tuple<double, double> end)
        {
            var pos1 = new GeoCoordinate(start.Item1, start.Item2);
            var pos2 = new GeoCoordinate(end.Item1, end.Item2);
            return pos1.GetDistanceTo(pos2);
        }
    }
}
