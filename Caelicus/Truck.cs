using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Caelicus.Graph;
using Caelicus.Services;
using Newtonsoft.Json;

namespace Caelicus
{
    public class Truck
    {
        //based on https://www.first-transport.dk/services/priser-og-betingelser

        private int payLoadWeight;

        public int totalCost;
        
        private Vertex<int,int> position; //This could maybe be the coordinates of the given vertex that the truck is on?

        private Vertex<int, int> goal;

        public int travelTime;

        //Cpst are dependant on payLoadcapacity, travelTime, and a minimum cost per startet journey 
        private int costPerTime;
        private int costPerDistance;

        private int distanceToBeTraveled;

        private int weight;

        private TruckType type;

        public int CostPerTime { get => costPerTime; set => costPerTime = value; }
        public int CostPerDistance { get => costPerDistance; set => costPerDistance = value; }

        public Truck(int payLoadWeight, int weight, Vertex<int,int> position, Vertex<int,int> goal) {   
            this.payLoadWeight = payLoadWeight;
            this.weight = weight;
            this.position = position;
            
            //Calculate costPer time/Distance based on first-transport.dk
            if (weight <= 3500) {
                costPerTime = 425 + (85 * (travelTime / 15)); //per 15. startet minute
                costPerDistance = 425 + (10 * distanceToBeTraveled);
                type = TruckType.Pickup;

            } else if (weight > 3500 && payLoadWeight <= 6) {
                costPerTime = 768 + (256 * (travelTime / 30)); //per 30. started minute
                costPerDistance = 768 + (10 * distanceToBeTraveled);
                type = TruckType.Lastvogn;


            } else if (weight > 3500 && payLoadWeight <= 14) {
                costPerTime = 852 + (284 * (travelTime / 30)); //per 30. started minute
                costPerDistance = 852 + (10 * distanceToBeTraveled);
                type = TruckType.StorLastvogn;

            } else if (weight > 3500 && payLoadWeight <= 33) {
                costPerTime = 1440 + (360 * (travelTime / 30)); //per 30. started minute
                costPerDistance = 852 + (13 * distanceToBeTraveled);
                type = TruckType.Sættevogn;
            }


        }

        public Vertex<int, int> goTo(Vertex<int, int> distiantion)
        {
            this.position = distiantion;
            return distiantion;
        }
    }

   
}
