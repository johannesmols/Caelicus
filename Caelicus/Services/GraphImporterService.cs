using System;
using System.Collections.Generic;
using System.Linq;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Helpers;
using Caelicus.Models.Graph;
using GoogleMapsComponents.Maps;
using Newtonsoft.Json;

namespace Caelicus.Services
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
            catch (Exception e)
            {
                Console.WriteLine($"Error while parsing graph JSON for a vertex type " +
                                  $"(type given was { type } but only { VertexType.Base } and " +
                                  $"{ VertexType.Target } are allowed)");
                
                return VertexType.Target;
            }
        }
    }
}
