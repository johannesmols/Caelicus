using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleMapsComponents.Maps;

namespace Caelicus.Models.Graph
{
    public class JsonEdge
    {
        public string Target { get; set; }
        public List<Mode> Modes { get; set; }
    }

    public class Mode
    {
        public string TravelMode { get; set; }
        public int Distance { get; set; }
        public int Time { get; set; }
    }
}
