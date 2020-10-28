using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Models;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Caelicus.Services
{
    public class AppState
    {
        public List<JsonGraphRootObject> Graphs { get; private set; } = new List<JsonGraphRootObject>();
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public List<Vehicle> VehicleTemplates { get; set; } = new List<Vehicle>();
        public void UpdateGraphs(ComponentBase source, List<JsonGraphRootObject> graphs)
        {
            Graphs = graphs;
            NotifyStateChanged(source, nameof(Graphs));
        }

        public event Action<ComponentBase, string> StateChanged;

        private void NotifyStateChanged(ComponentBase source, string property) => StateChanged?.Invoke(source, property);
    }
}
