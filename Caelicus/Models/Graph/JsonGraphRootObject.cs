using System.Collections.Generic;

namespace Caelicus.Models.Graph
{
    public class JsonGraphRootObject
    {
        public string Name { get; set; }
        public List<JsonVertex> Vertices { get; set; }

        public JsonGraphRootObject()
        {
            Name = string.Empty;
            Vertices = new List<JsonVertex>();
        }
    }
}
