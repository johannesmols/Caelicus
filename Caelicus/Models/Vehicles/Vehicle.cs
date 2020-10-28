using System.Runtime.Versioning;

namespace Caelicus.Models.Vehicles
{
    public class Vehicle
    {

        // General information
        public string Name { get; set; }
        public bool isActive { get; set; } = true;

        // Movement
        public double Speed { get; set; }
        public double MaxRange { get; set; }
        public double MovementPenalty { get; set; }

        // Transport
        public double MaxPayload { get; set; }
        
        // Cost
        public double CostPerHour { get; set; }
        public double CostPerKm { get; set; }

        //with copy constructor the json import fails
        public Vehicle copy()
        {
            Vehicle rtn = new Vehicle();
            rtn.Name = Name;
            rtn.isActive = isActive;
            rtn.Speed = Speed;
            rtn.MaxRange = MaxRange;
            rtn.MovementPenalty = MovementPenalty;
            rtn.MaxPayload = MaxPayload;
            rtn.CostPerHour = CostPerHour;
            rtn.CostPerKm = CostPerKm;
            return rtn;
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
    }
}
