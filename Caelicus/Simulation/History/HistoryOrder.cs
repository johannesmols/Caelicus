using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation.History
{
    public class HistoryOrder
    {
        public string Start { get; set; }
        public string Target { get; set; }
        public double? PayloadWeight { get; set; }

        public HistoryOrder(string start, string target, double? payloadWeight)
        {
            Start = start;
            Target = target;
            PayloadWeight = payloadWeight;
        }

        public HistoryOrder(HistoryOrder order)
        {
            Start = order.Start;
            Target = order.Target;
            PayloadWeight = order.PayloadWeight;
        }

        public HistoryOrder()
        {
        }
    }
}
