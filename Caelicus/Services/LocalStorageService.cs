﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorApp.Services.GoogleMapsDistanceMatrix.Internal;
using Blazored.LocalStorage;
using SimulationCore.Models.Graph;
using SimulationCore.Models.Vehicles;

namespace BlazorApp.Services
{
    public class LocalStorageService
    {
        private readonly ILocalStorageService _localStorage;

        public LocalStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }
        
        
        // Graphs

        public async Task<List<JsonGraphRootObject>> GetLocalStorageGraphs()
        {
            var localStorageGraphs = new List<JsonGraphRootObject>();

            for (var i = 0; i < await _localStorage.LengthAsync(); i++)
            {
                var key = await _localStorage.KeyAsync(i);
                JsonGraphRootObject jsonObject = null;

                try
                {
                    jsonObject = await _localStorage.GetItemAsync<JsonGraphRootObject>(key);
                    if (!(jsonObject.Vertices?.Count > 0))
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not convert local storage object with key '{ key }' to graph object, ignoring object.");
                }

                if (jsonObject != null)
                {
                    localStorageGraphs.Add(jsonObject);
                }
            }

            return localStorageGraphs;
        }

        public async Task WriteGraphsToLocalStorage(IList<JsonGraphRootObject> graphs)
        {
            foreach (var graph in graphs)
            {
                await _localStorage.SetItemAsync(graph.Name, graph);
            }
        }

        /// <summary>
        /// Get stats about the graphs queried from the Google Maps API
        /// </summary>
        /// <returns>a list of tuples, one containing the key/name of the graph, and the other the values</returns>
        public async Task<List<Tuple<string, Dictionary<Route, RouteStats>>>> GetLocalStorageGraphStats()
        {
            var localStorageGraphStats = new List<Tuple<string, Dictionary<Route, RouteStats>>>();

            for (var i = 0; i < await _localStorage.LengthAsync(); i++)
            {
                var key = await _localStorage.KeyAsync(i);
                Dictionary<Route, RouteStats> jsonObject = null;

                try
                {
                    jsonObject = await _localStorage.GetItemAsync<Dictionary<Route, RouteStats>>(key);
                    if (!(jsonObject?.Count > 0))
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not convert local storage object with key '{ key }' to graph stats object, ignoring object.");
                }

                if (jsonObject != null)
                {
                    localStorageGraphStats.Add(Tuple.Create(key, jsonObject));
                }
            }

            return localStorageGraphStats;
        }

        public async Task WriteGraphStatsToLocalStorage(string key, Dictionary<Route, RouteStats> graphStats)
        {
            await _localStorage.SetItemAsync(key, graphStats);
        }


        // Vehicles

        public async Task<List<Vehicle>> GetLocalVehicles()
        {
            var localStorageVehicles = new List<Vehicle>();

            for (var i = 0; i < await _localStorage.LengthAsync(); i++)
            {
                var key = await _localStorage.KeyAsync(i);
                Vehicle jsonObject = null;

                try
                {
                    jsonObject = await _localStorage.GetItemAsync<Vehicle>(key);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Could not convert local storage object with key '{ key }' to vehicle object, ignoring object.");
                }

                if (jsonObject != null)
                {
                    localStorageVehicles.Add(jsonObject);
                }
            }

            return localStorageVehicles;
        }

        public async Task WriteVehiclesToLocalStorage(IList<Tuple<Vehicle, bool, int, int, int>> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                await _localStorage.SetItemAsync(vehicle.Item1.Name, vehicle.Item1);
            }
        }
    }
}
