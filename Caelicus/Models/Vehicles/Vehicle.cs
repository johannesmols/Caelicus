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
            FuelConsumption = vehicle.FuelConsumption;
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
        public double FuelConsumption { get; set; }
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
        /// Not considering the average speed as it is quite complicated (e.g. http://www.pv4.eu/calculate-fuel-consumption-drag-coefficient-speed-and-weight_912.html)
        /// </summary>
        /// <returns></returns>
        public double GetMaximumTravelDistance()
        {
            return FuelCapacity / FuelConsumption;
        }

        public double GetMaximumTravelDistance(double payload)
        {
            // TODO: Account for payload somehow
            return FuelCapacity / FuelConsumption;
        }
    }
}
