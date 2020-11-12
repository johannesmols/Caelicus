using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BlazorLeaflet;
using BlazorLeaflet.Models;
using Caelicus.Enums;
using Caelicus.Graph;
using Caelicus.Models.Graph;
using Microsoft.JSInterop;

namespace Caelicus.Services
{
    public class LeafletMapRenderService
    {
        private readonly IJSRuntime _jsRuntime;
        private Map _map;
        private bool _initialized;

        private readonly List<Circle> _vertices = new List<Circle>();
        private readonly List<Polyline> _paths = new List<Polyline>();

        public LeafletMapRenderService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Map Initialize()
        {
            _map = new Map(_jsRuntime)
            {
                Center = new LatLng(55.678074f, 12.572356f),
                Zoom = 8.5f
            };

            _map.OnInitialized += AddOsmLayer;

            _initialized = true;

            return _map;
        }

        private void AddOsmLayer()
        {
            _map.AddLayer(new TileLayer
            {
                UrlTemplate = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                Attribution = "&copy; <a href=\"https://www.openstreetmap.org/copyright\">OpenStreetMap</a> contributors",
            });
        }

        public void RenderGraph(Graph<VertexInfo, EdgeInfo> graph)
        {
            if (!_initialized)
                Initialize();

            ClearMap();

            // Add markers for each vertex
            foreach (var vertex in graph.Vertices)
            {
                _vertices.Add(new Circle()
                {
                    Position = new LatLng((float)vertex.Info.Position.Item1, (float)vertex.Info.Position.Item2),
                    Radius = 1f,
                    StrokeColor = vertex.Info.Type == VertexType.Target ? Color.Red : Color.Green,
                    StrokeWidth = 7
                });

                // Add lines for each edge
                foreach (var edge in vertex.Edges)
                {
                    _paths.Add(new Polyline()
                    {
                        Shape = new[]
                        {
                            new []
                            {
                                new PointF((float) edge.Origin.Info.Position.Item1, (float) edge.Origin.Info.Position.Item2), 
                                new PointF((float) edge.Destination.Info.Position.Item1, (float) edge.Destination.Info.Position.Item2)
                            }
                        },
                        StrokeColor = Color.DodgerBlue
                    });
                }
            }

            _paths.ForEach(_map.AddLayer);
            _vertices.ForEach(_map.AddLayer);

            PanToPoint(_vertices.First().Position);
        }

        public void PanToPoint(LatLng point, float zoom = 0f)
        {
            if (!_initialized)
                Initialize();

            _map.PanTo(new PointF(point.Lat, point.Lng), true, 1f);
        }

        public void ClearMap()
        {
            if (!_initialized)
                Initialize();

            _vertices.Clear();
            _paths.Clear();

            _map.GetLayers().ToList().ForEach(l => _map.RemoveLayer(l));
            AddOsmLayer();
        }
    }
}
