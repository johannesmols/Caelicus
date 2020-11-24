using System;
using System.Collections.Generic;
using GoogleMapsComponents.Maps;

namespace SimulationCore.Models.Graph
{
    public class EdgeInfo
    {
        public double Distance { get; set; }
        public Dictionary<TravelMode, Tuple<double, double>> GMapsDistanceAndTime { get; set; }
    }
}
