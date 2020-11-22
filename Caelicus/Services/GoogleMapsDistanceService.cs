using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caelicus.Services.GoogleMapsDistanceMatrix.Internal;
using GoogleMapsComponents.Maps;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Caelicus.Services
{
    public static class GoogleMapsDistanceService
    {
        private static IJSRuntime _js;
        private static Dictionary<Route, RouteStats> _distancesAndTime = new Dictionary<Route, RouteStats>();
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static List<LatLng> _origins;
        private static List<LatLng> _destinations;
        private static TravelMode _mode;

        public static void Init(IJSRuntime runtime)
        {
            _js = runtime;
        }

        /// <summary>
        /// Add Distance to be calculated.
        /// All possible combinations of origins and destinations are calculated
        /// </summary>
        /// <param name="mode">Select the type of Travel</param>
        /// <param name="origins">List of origins for the calculations</param>
        /// <param name="destinations">List of destinations used for the calculation</param>
        /// <returns></returns>
        public static async Task CalculateDistances(TravelMode mode, List<LatLng> origins, List<LatLng> destinations)
        {
            if (_js is null) return;

            await _semaphore.WaitAsync();
            _mode = mode;
            _origins = origins;
            _destinations = destinations;
            var matrix = new OriginDestinationMatrix
            {
                Origins = origins,
                Destinations = destinations,
                TravelMode = _mode switch
                {
                    TravelMode.Driving => "DRIVING",
                    TravelMode.Walking => "WALKING",
                    TravelMode.Bicycling => "BICYCLING",
                    TravelMode.Transit => "TRANSIT",
                    _ => "DRIVING"
                }
            };

            var json = JsonConvert.SerializeObject(matrix);
            await _js.InvokeAsync<string>("getDistance", json);
        }

        [JSInvokable("GoogleMapsDistanceCallback")]
        public static void GoogleMapsDistanceCallback(object resp)
        {
            var jsonResponse = resp.ToString();
            var matrix = JsonConvert.DeserializeObject<JsonGoogleMapsDistanceMatrix>(jsonResponse ?? string.Empty);
            AddDistanceMatrix(matrix);
            _semaphore.Release();
        }

        private static void AddDistanceMatrix(JsonGoogleMapsDistanceMatrix jsonGoogleMaps)
        {
            //iterate over origins
            for (var i = 0; i < jsonGoogleMaps.Rows.Count; i++)
            {
                var origin = _origins.ElementAt(i);

                //iterate over all possible destinations for the current origin
                for (var j = 0; j < jsonGoogleMaps.Rows.ElementAt(i).Elements.Count; j++)
                {
                    if (jsonGoogleMaps.Rows.ElementAt(i).Elements.ElementAt(j).Status == "OK")
                    {
                        var destination = _destinations.ElementAt(j);
                        var route = new Route(_mode, origin, destination);
                        if (!_distancesAndTime.ContainsKey(route))
                        {
                            var stats = new RouteStats(jsonGoogleMaps.Rows.ElementAt(i).Elements.ElementAt(j).Distance.Value, jsonGoogleMaps.Rows.ElementAt(i).Elements.ElementAt(j).Duration.Value);
                            _distancesAndTime.Add(route, stats);
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
            if (_js is null) return null;

            await _semaphore.WaitAsync();
            var route = new Route(mode, origin, destination);

            if (_distancesAndTime.TryGetValue(route, out var res))
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
        public double Distance;
        public double Time;

        public RouteStats(double distance, double time)
        {
            Distance = distance;
            Time = time;
        }
    }

    public struct LatLng
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public LatLng(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }
    }

    namespace GoogleMapsDistanceMatrix.Internal
    {
        //structure to convert to json and transfer to javascript
        public class OriginDestinationMatrix
        {
            public List<LatLng> Origins { get; set; } = new List<LatLng>();
            public List<LatLng> Destinations { get; set; } = new List<LatLng>();
            public string TravelMode = "DRIVING";
        }


        //structure to deserialize json return of Distance Matrix API
        public class Distance
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class Duration
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class Element
        {
            public Distance Distance { get; set; }
            public Duration Duration { get; set; }
            public string Status { get; set; }
        }

        public class Row
        {
            public List<Element> Elements { get; set; }
        }

        public class JsonGoogleMapsDistanceMatrix
        {
            public List<Row> Rows { get; set; }
            public List<string> OriginAddresses { get; set; }
            public List<string> DestinationAddresses { get; set; }
        }

        //struct so it can be used easily as a key in dictionary
        public readonly struct Route
        {
            public readonly TravelMode Mode;
            public readonly LatLng Origin;
            public readonly LatLng Destination;

            public Route(TravelMode mode, LatLng origin, LatLng destination)
            {
                Origin = origin;
                Destination = destination;
                Mode = mode;
            }
        }
    }
}
