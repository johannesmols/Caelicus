using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using SimulationCore.Enums;
using SimulationCore.Graph;
using SimulationCore.Models.Graph;

namespace BlazorApp.Services
{
    public class MapRenderService
    {
        private readonly GoogleMap _map;

        private List<Marker> _markers = new List<Marker>();
        private List<Polyline> _polylines = new List<Polyline>();

        public MapRenderService(GoogleMap map)
        {
            _map = map;
        }

        public async Task RenderMap(Graph<VertexInfo, EdgeInfo> graph)
        {
            await ClearMap();

            // Add markers for each vertex
            foreach (var vertex in graph.Vertices)
            {
                _markers.Add(await Marker.CreateAsync(_map.JsRuntime, new MarkerOptions()
                {
                    Map = _map.InteropObject,
                    Position = new LatLngLiteral(vertex.Info.Position.Item2, vertex.Info.Position.Item1),
                    Clickable = false,
                    //Label = vertex.Info.Name,
                    Icon = new Icon()
                    {
                        Url = vertex.Info.Type == VertexType.Target ?  "icons/circle_green.svg" : "icons/home_black.svg",
                        ScaledSize = new Size()
                        {
                            Height = 24,
                            Width = 24
                        }
                    }
                }));

                // Add lines for each edge
                foreach (var edge in vertex.Edges)
                {
                    _polylines.Add(await Polyline.CreateAsync(_map.JsRuntime, new PolylineOptions()
                    {
                        Map = _map.InteropObject,
                        Path = new[]
                        {
                            new LatLngLiteral(edge.Origin.Info.Position.Item2, edge.Origin.Info.Position.Item1),
                            new LatLngLiteral(edge.Destination.Info.Position.Item2, edge.Destination.Info.Position.Item1)
                        },
                        StrokeWeight = 2,
                        StrokeColor = "#ff0000"
                    }));
                }
            }

            await PanToPoint(new LatLngLiteral(graph.Vertices.First().Info.Position.Item2, graph.Vertices.First().Info.Position.Item1));
        }

        public async Task PanToPoint(LatLngLiteral point)
        {
            await _map.InteropObject.PanTo(point);
        }

        private async Task ClearMap()
        {
            if (_markers != null)
            {
                foreach (var marker in _markers.Reverse<Marker>())
                {
                    await marker.SetMap(null);
                    _markers.Remove(marker);
                }
            }

            if (_polylines != null)
            {
                foreach (var polyline in _polylines.Reverse<Polyline>())
                {
                    await polyline.SetMap(null);
                    _polylines.Remove(polyline);
                }
            }
        }
    }
}
