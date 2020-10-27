using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Caelicus.Models;
using Caelicus.Models.Graph;

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
    }
}
