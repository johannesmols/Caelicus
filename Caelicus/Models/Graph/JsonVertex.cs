using System.Collections.Generic;

namespace Caelicus.Models.Graph
{
    public class JsonVertex
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Edges { get; set; }
    }
}
