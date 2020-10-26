using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Services;

namespace Caelicus.Models
{
    public class GraphRootObjectJson
    {
        public string Name { get; set; }
        public List<JsonVertex> Vertices { get; set; }

        public GraphRootObjectJson()
        {
            Name = string.Empty;
            Vertices = new List<JsonVertex>();
        }
    }
}
