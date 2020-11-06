using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;
using Caelicus.Services;
using Caelicus.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;


namespace SimulationTests
{
    [TestClass]
    public class SimulationUnittest
    {
        private SimulationParameters _params = new SimulationParameters();
        
        [TestMethod]
        public async Task loadJsonResources()
        {
            // To load the data from the data resource just declare the resource file as txt file and load it
            // by invoking data.file
            var vehicle = JsonConvert.DeserializeObject<List<Vehicle>>(data.vehicles);
            var graph = JsonConvert.DeserializeObject<JsonGraphRootObject>(data.copenhagen);
            _params = new SimulationParameters() 
             {
                  SimulationIdentifier = Guid.NewGuid(),
                  RandomSeed = 42,
                  Graph = new GraphImporterService().GenerateGraph(graph),
                  VehicleTemplate = vehicle[0],
                  NumberOfVehicles = 1,
                  SimulationSpeed = 10
             };
            Assert.AreEqual("Copenhagen", graph.Name); 
            Assert.AreEqual("Drone", vehicle[0].Name);
        }
    }
}
