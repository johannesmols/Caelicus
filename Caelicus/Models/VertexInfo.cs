using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caelicus.Enums;

namespace Caelicus.Models
{
    public class VertexInfo
    {
        public string Name { get; set; }
        public VertexType Type { get; set; }
        public Tuple<double, double> Position { get; set; }
    }
}
