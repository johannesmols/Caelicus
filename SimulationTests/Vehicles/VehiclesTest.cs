using Caelicus.Models.Vehicles;
using System;
using Xunit;



namespace SimulationTests
{
    public class VehiclesTest
    {
       
        [Fact]
        public void CalculateJourneyCostTest()
        {

            var basicTestVihecle = new Vehicle();

            int testVarible = 2;
            int testDistance = 2000;

            basicTestVihecle.CostPerHour = testVarible; 
            basicTestVihecle.CostPerKm = testVarible;
            basicTestVihecle.Speed = testVarible;
            
            int expectedResult = 6;

            
            Assert.Equal(expectedResult,basicTestVihecle.CalculateJourneyCost(testDistance));
         
        }

        [Fact]
        public void basicInitialization() {

            int testVariable = 1;
            var basicTestVihecle = new Vehicle();
            basicTestVihecle.CostPerHour = testVariable;
            basicTestVihecle.CostPerKm = testVariable;
            basicTestVihecle.MaxPayload = testVariable;
            basicTestVihecle.MaxRange = testVariable;
            basicTestVihecle.MovementPenalty = testVariable;
            basicTestVihecle.Name = "BASIC";

            Assert.Equal(testVariable, basicTestVihecle.CostPerHour);
            Assert.Equal(testVariable, basicTestVihecle.CostPerKm);
            Assert.Equal(testVariable, basicTestVihecle.MaxPayload);
            Assert.Equal(testVariable, basicTestVihecle.MaxRange);
            Assert.Equal(testVariable, basicTestVihecle.MovementPenalty);
            Assert.Contains("BASIC", basicTestVihecle.Name);
        }

    }
}
