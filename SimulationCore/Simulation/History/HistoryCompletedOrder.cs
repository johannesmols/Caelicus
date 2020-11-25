using System.Collections.Generic;

namespace SimulationCore.Simulation.History
{
    public class HistoryCompletedOrder : HistoryOrder
    {
        public double? PickupTime { get; set; }

        public double? PickupDistance { get; set; }

        public double? PickupCost { get; set; }

        public double? DeliveryTime { get; set; }

        public double? DeliveryDistance { get; set; }

        public double? DeliveryCost { get; set; }

        public List<string> DeliveryPath { get; set; }

        public HistoryCompletedOrder(HistoryOrder order, double? deliveryTime, double? deliveryDistance, double? deliveryCost, double? pickupTime, double? pickupDistance, double? pickupCost, List<string> deliveryPath) : base(order)
        {
            DeliveryTime = deliveryTime;
            DeliveryDistance = deliveryDistance;
            DeliveryCost = deliveryCost;
            PickupTime = pickupTime;
            PickupDistance = PickupDistance;
            PickupCost = pickupCost;
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
