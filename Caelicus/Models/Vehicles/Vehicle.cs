using System;
using Caelicus.Graph;
using Caelicus.Models.Graph;

namespace Caelicus.Models.Vehicles
{
    
    public class Vehicle
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

        public Vehicle(Vehicle v)
        {
            this.Name = v.Name;
            this.Speed = v.Speed;
            this.MaxPayload = v.MaxPayload;
            this.MaxRange = v.MaxRange;
            this.MovementPenalty = v.MovementPenalty;
            this.CostPerHour = v.CostPerHour;
            this.CostPerKm = v.CostPerKm;
        }
    }
}
