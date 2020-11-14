using System;
using Caelicus.Graph;
using Caelicus.Models.Graph;

namespace Caelicus.Models.Vehicles
{
    
    public class Vehicle
    {
        public Vehicle()
        {

        }

        protected Vehicle(Vehicle vehicle)
        {
            Name = vehicle.Name;
            AverageSpeed = vehicle.AverageSpeed;
            MaxPayload = vehicle.MaxPayload;
            FuelCapacity = vehicle.FuelCapacity;
            BaseFuelConsumption = vehicle.BaseFuelConsumption;
            ExtraFuelConsumptionPerKg = vehicle.ExtraFuelConsumptionPerKg;
            RefuelingTime = vehicle.RefuelingTime;
            AllowRefuelAtTarget = vehicle.AllowRefuelAtTarget;
            PurchasingCost = vehicle.PurchasingCost;
            CostPerHour = vehicle.CostPerHour;
            CostPerKm = vehicle.CostPerKm;
        }

        // General information
        public string Name { get; set; }

        // Movement
        public double AverageSpeed { get; set; }
        public double FuelCapacity { get; set; }
        public double BaseFuelConsumption { get; set; }
        public double ExtraFuelConsumptionPerKg { get; set; }
        public double RefuelingTime { get; set; }
        public bool AllowRefuelAtTarget { get; set; }

        // Transport
        public double MaxPayload { get; set; }
        
        // Cost
        public double PurchasingCost { get; set; }
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
            var travelTimeInHours = (distanceInMetres / 1000d) / AverageSpeed;
            var baseHourlyCost = CostPerHour * travelTimeInHours;
            var baseDistanceCost = CostPerKm * (distanceInMetres / 1000d);
            return baseHourlyCost + baseDistanceCost;
        }

        /// <summary>
        /// Calculate how far the vehicle can travel given the fuel capacity and fuel consumption.
        /// </summary>
        /// <returns></returns>
        public double GetMaximumTravelDistance()
        {
            return FuelCapacity / BaseFuelConsumption;
        }

        /// <summary>
        /// Calculate how far the vehicle can travel given the fuel capacity, fuel consumption, and payload weight.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public double GetMaximumTravelDistance(double payload)
        {
            return FuelCapacity / (BaseFuelConsumption + ExtraFuelConsumptionPerKg * payload);
        }
    }
}
