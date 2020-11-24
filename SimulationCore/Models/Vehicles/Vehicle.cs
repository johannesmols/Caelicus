using System.ComponentModel;
using GoogleMapsComponents.Maps;

namespace SimulationCore.Models.Vehicles
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
            TravelMode = vehicle.TravelMode;
        }

        // General information
        public string Name { get; set; }

        // Movement
        [Description("km/h")]
        public double AverageSpeed { get; set; }

        [Description("liters, mAh, etc.")]
        public double FuelCapacity { get; set; }

        [Description("per meter")]
        public double BaseFuelConsumption { get; set; }

        [Description("per kg per meter")]
        public double ExtraFuelConsumptionPerKg { get; set; }

        [Description("seconds")]
        public double RefuelingTime { get; set; }

        [Description("true/false")]
        public bool AllowRefuelAtTarget { get; set; }

        // Transport
        [Description("kg")]
        public double MaxPayload { get; set; }
        
        // Cost
        [Description("DKK")]
        public double PurchasingCost { get; set; }

        [Description("DKK")]
        public double CostPerHour { get; set; }

        [Description("DKK")]
        public double CostPerKm { get; set; }

        public TravelMode TravelMode { get; set; }

        /// <summary>
        /// Calculate the cost of a journey given a distance.
        /// The base hourly cost plus the cost per kilometre is considered.
        /// </summary>
        /// <param name="distanceInMetres">distance in metres</param>
        /// <param name="travelTime">travel time in seconds</param>
        /// <returns></returns>
        public double CalculateJourneyCost(double distanceInMetres, double travelTime = 0d)
        {
            var travelTimeInHours = travelTime != 0d ? (distanceInMetres / 1000d) / AverageSpeed : travelTime / 60d / 60d;
            var baseHourlyCost = CostPerHour * travelTimeInHours;
            var baseDistanceCost = CostPerKm * (distanceInMetres / 1000d);
            return baseHourlyCost + baseDistanceCost;
        }

        public double GetSpeedInMetersPerSecond()
        {
            return AverageSpeed / 3.6d;
        }

        public double GetFuelConsumptionForOneMeter(double payload)
        {
            return BaseFuelConsumption + ExtraFuelConsumptionPerKg * payload;
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

        public double GetMaximumTravelDistance(double payload, double fuelLoaded)
        {
            return fuelLoaded / (BaseFuelConsumption + ExtraFuelConsumptionPerKg * payload);
        }
    }
}
