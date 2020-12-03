using System.Collections.Generic;

namespace SimulationCore.Models.Graph
{
    public class JsonEdge
    {
        public string Target { get; set; }
        public List<Mode> Modes { get; set; } = new List<Mode>();
    }

    public class Mode
    {
        public string TravelMode { get; set; }
        public int Distance { get; set; }
        public int Time { get; set; }
    }
}
