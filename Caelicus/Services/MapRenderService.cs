using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Models;
using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using OneOf;

namespace Caelicus.Services
{
    public class MapRenderService
    {
        public static async Task RenderMap(GoogleMap map, Graph<VertexInfo, EdgeInfo> graph)
        {
            // Add markers for each vertex
            foreach (var vertex in graph.Vertices)
            {
                await Marker.CreateAsync(map.JsRuntime, new MarkerOptions()
                {
                    Map = map.InteropObject,
                    Position = new LatLngLiteral(vertex.Info.Position.Item2, vertex.Info.Position.Item1),
                    Clickable = false,
                    //Label = vertex.Info.Name,
                    Icon = new Icon()
                    {
                        Url = vertex.Info.Type == VertexType.Target ?  "/icons/circle_green.svg" : "/icons/home_black.svg",
                        ScaledSize = new Size()
                        {
                            Height = 24,
                            Width = 24
                        }
                    }
                });

                // Add lines for each edge
                foreach (var edge in vertex.Edges)
                {
                    await Polyline.CreateAsync(map.JsRuntime, new PolylineOptions()
                    {
                        Map = map.InteropObject,
                        Path = new[]
                        {
                            new LatLngLiteral(edge.Origin.Info.Position.Item2, edge.Origin.Info.Position.Item1),
                            new LatLngLiteral(edge.Destination.Info.Position.Item2, edge.Destination.Info.Position.Item1)
                        },
                        StrokeWeight = 2,
                        StrokeColor = "#ff0000"
                    });
                }
            }

            await PanToPoint(map, new LatLngLiteral(graph.Vertices.First().Info.Position.Item2, graph.Vertices.First().Info.Position.Item1));
        }

        public static async Task PanToPoint(GoogleMap map, LatLngLiteral point)
        {
            await map.InteropObject.PanTo(point);
        }

        private static async Task ClearMap(GoogleMap map)
        {
            // TODO: implement
        }
    }
}
