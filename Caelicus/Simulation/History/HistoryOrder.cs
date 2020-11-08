using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation.History
{
    public class HistoryOrder
    {
        public Guid? Start { get; set; }
        public Guid? Target { get; set; }
        public double? PayloadWeight { get; set; }

        public HistoryOrder(Guid? start, Guid? target, double? payloadWeight)
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
