using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Caelicus.Services
{
    public class AppState
    {
        public List<JsonRootObject> Graphs { get; private set; } = new List<JsonRootObject>();

        public void UpdateGraphs(ComponentBase source, List<JsonRootObject> graphs)
        {
            Graphs = graphs;
            NotifyStateChanged(source, nameof(Graphs));
        }

        public event Action<ComponentBase, string> StateChanged;

        private void NotifyStateChanged(ComponentBase source, string property) => StateChanged?.Invoke(source, property);
    }
}
