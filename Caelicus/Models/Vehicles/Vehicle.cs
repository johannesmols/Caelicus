using System;
using Caelicus.Graph;
using Caelicus.Models.Graph;

namespace Caelicus.Models.Vehicles
{
    public enum VehicleState
    {
        Moving,
        Steady
    }
    
    public class Vehicle : VertexInfo
    {
        // General information
        public string Name { get; set; }

        // Movement
        public double Speed { get; set; }
        public double MaxRange { get; set; }
        public double MovementPenalty { get; set; }

        // Transport
        public double MaxPayload { get; set; }
        
        // Cost
        public double CostPerHour { get; set; }
        public double CostPerKm { get; set; }
        
        private VertexInfo Target { get; set; }
        
        private VertexInfo Base { get; set; }
        
        private VehicleState State { get; set; } 

        /// <summary>
        /// Check if we arrived at the destination. Since the coordinates are floating points we should get distance
        /// between two points and compare it with a minimum threshold. The threshold for now is 0.10 but I think it
        /// is quite large!!.
        /// </summary>
        /// <returns></returns>
        public bool ArrivedToTarget()
        {
            return Math.Sqrt(Math.Pow(this.Position.Item1 - Target.Position.Item1, 2) + 
                Math.Pow(this.Position.Item2 - Target.Position.Item2, 2)) >= 0.10;
        }

        /// <summary>
        /// Calculate the cost of a journey given a distance.
        /// The base hourly cost plus the cost per kilometre is considered.
        /// </summary>
        /// <param name="distanceInMetres"></param>
        /// <returns></returns>
        public double CalculateJourneyCost(double distanceInMetres)
        {
            var travelTimeInHours = (distanceInMetres / 1000d) / Speed;
            var baseHourlyCost = CostPerHour * travelTimeInHours;
            var baseDistanceCost = CostPerKm * (distanceInMetres / 1000d);
            return baseHourlyCost + baseDistanceCost;
        }

        public void SetTarget(VertexInfo t)
        {
            Target = t;
        }
    }
}
