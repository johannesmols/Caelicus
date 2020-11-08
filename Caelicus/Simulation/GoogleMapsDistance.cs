using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Caelicus.Simulation.GoogleMapsDistanceMatrix.Internal;
using System.Threading;
using GoogleMapsComponents;
using GoogleMapsComponents.Maps;

namespace Caelicus.Simulation
{
    public static class GoogleMapsDistance
    {
        private static IJSRuntime JS;
        private static Dictionary<Route, RouteStats> _distances_and_time = new Dictionary<Route, RouteStats>();
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static List<LatLng> _origins;
        private static List<LatLng> _destinations;
        private static TravelMode _mode;
        public static void init(IJSRuntime runtime)
        {
            JS = runtime;
        }

        /// <summary>
        /// Add Distance to be calculated.
        /// All possible combinations of origins and destinations are calculated
        /// </summary>
        /// <param name="mode">Select the typ of Travel</param>
        /// <param name="origins">List of origins for the calculations</param>
        /// <param name="destinations">List of destinations used for the calculation</param>
        /// <returns></returns>
        public static async Task CalculateDistances(TravelMode mode, List<LatLng> origins, List<LatLng> destinations)
        {
            await _semaphore.WaitAsync();
            _mode = mode;
            _origins = origins;
            _destinations = destinations;
            var matrix = new OriginDestinationMatrix();
            matrix.origins = origins;
            matrix.destinations = destinations;
            switch (_mode)
            {
                case TravelMode.Driving:
                    matrix.TravelMode = "DRIVING";
                    break;
                case TravelMode.Walking:
                    matrix.TravelMode = "WALKING";
                    break;
                case TravelMode.Bicycling:
                    matrix.TravelMode = "BICYCLING";
                    break;
                case TravelMode.Transit:
                    matrix.TravelMode = "TRANSIT";
                    break;
                default:
                    matrix.TravelMode = "DRIVING";
                    break;
            }
            var json = JsonConvert.SerializeObject(matrix);
            await JS.InvokeAsync<string>("getDistance", json);
        }

        [JSInvokable("GoogleMapsDistanceCallback")]
        public static void GoogleMapsDistanceCallback(object resp)
        {
            var json_response = resp.ToString();
            var matrix = JsonConvert.DeserializeObject<JsonGoogleMapsDistanceMatrix>(json_response);
            addDistanceMatrix(matrix);
            _semaphore.Release();
        }

        private static void addDistanceMatrix(JsonGoogleMapsDistanceMatrix jsonGoogleMaps)
        {
            //iterate over origins
            for (int i = 0; i < jsonGoogleMaps.rows.Count; i++)
            {
                LatLng origin = new LatLng(_origins.ElementAt(i).lat, _origins.ElementAt(i).lng);
                //iterate over all possible destinations for the current origin
                for (int j = 0; j < jsonGoogleMaps.rows.ElementAt(i).elements.Count; j++)
                {
                    if (jsonGoogleMaps.rows.ElementAt(i).elements.ElementAt(j).status == "OK")
                    {
                        LatLng destination = new LatLng(_destinations.ElementAt(i).lat, _destinations.ElementAt(i).lng);
                        var route = new Route(_mode, origin, destination);
                        if (!_distances_and_time.ContainsKey(route))
                        {
                            RouteStats stats = new RouteStats(jsonGoogleMaps.rows.ElementAt(i).elements.ElementAt(j).distance.value, jsonGoogleMaps.rows.ElementAt(i).elements.ElementAt(j).duration.value);
                            _distances_and_time.Add(route, stats);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Query the distance and time for a specified Route.
        /// Returns Null if route not calculated yet
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns>RouteStats with time and distance or Null if route was not calculated</returns>
        public static async Task<RouteStats> GetDistanceAndTime(TravelMode mode, LatLng origin, LatLng destination)
        {
            await _semaphore.WaitAsync();
            var route = new Route(mode, origin, destination);
            RouteStats res;
            if (_distances_and_time.TryGetValue(route, out res))
            {
                _semaphore.Release();
                return res;
            }
            _semaphore.Release();
            return null;
        }
    }

    public class RouteStats
    {
        public double distance;
        public double time;
        public RouteStats(double distance, double time)
        {
            this.distance = distance;
            this.time = time;
        }
    }
    public struct LatLng
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public LatLng(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }
    }
    namespace GoogleMapsDistanceMatrix.Internal
    {
        //structure to convert to json and transfer to javascript
        public class OriginDestinationMatrix
        {
            public List<LatLng> origins { get; set; } = new List<LatLng>();
            public List<LatLng> destinations { get; set; } = new List<LatLng>();
            public string TravelMode = "DRIVING";
        }


        //structure to desierialize json return of Distance Matrix API
        public class Distance
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class Duration
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class Element
        {
            public Distance distance { get; set; }
            public Duration duration { get; set; }
            public string status { get; set; }
        }

        public class Row
        {
            public List<Element> elements { get; set; }
        }

        public class JsonGoogleMapsDistanceMatrix
        {
            public List<Row> rows { get; set; }
            public List<string> originAddresses { get; set; }
            public List<string> destinationAddresses { get; set; }
        }

        //struct so it can be used easily as a key in dictionary
        struct Route
        {
            TravelMode Mode;
            LatLng origin;
            LatLng destination;
            public Route(TravelMode travelmode, LatLng origin, LatLng destination)
            {
                this.origin = origin;
                this.destination = destination;
                this.Mode = travelmode;
            }
        }
    }
}
