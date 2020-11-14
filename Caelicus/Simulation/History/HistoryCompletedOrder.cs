using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Simulation.History
{
    public class HistoryCompletedOrder : HistoryOrder
    {
        public double? DeliveryTime { get; set; }

        public double? DeliveryDistance { get; set; }

        public List<string> DeliveryPath { get; set; }

        public HistoryCompletedOrder(HistoryOrder order, double? deliveryTime, double? deliveryDistance, List<string> deliveryPath) : base(order)
        {
            DeliveryTime = deliveryTime;
            DeliveryDistance = deliveryDistance;
            DeliveryPath = deliveryPath;
        }

        public HistoryCompletedOrder(HistoryOrder order) : base(order)
        {
        }

        public HistoryCompletedOrder()
        {

        }
    }
}
