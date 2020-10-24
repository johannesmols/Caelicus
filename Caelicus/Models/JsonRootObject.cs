using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Services;

namespace Caelicus.Models
{
    public class JsonRootObject
    {
        public string Name { get; set; }
        public List<JsonVertex> Vertices { get; set; }

        public JsonRootObject()
        {
            Name = string.Empty;
            Vertices = new List<JsonVertex>();
        }
    }
}
