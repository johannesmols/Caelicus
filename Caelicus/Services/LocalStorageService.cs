using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Caelicus.Models;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Services
{
    public class LocalStorageService
    {
        private readonly ILocalStorageService _localStorage;

        public LocalStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

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
