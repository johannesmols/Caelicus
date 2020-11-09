using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;
using Caelicus.Services;
using Caelicus.Simulation;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace SimulationTests
{
    [TestClass]
    public class SimulationTests
    {
        // Test fixture to pass around various tests
        // I found this singleton class online, it should be thread safe... 
        public sealed class SimResources
        {
            private static readonly SimResources instance = new SimResources();

            public List<Vehicle> Vehicle { get; } = JsonConvert.DeserializeObject<List<Vehicle>>(data.vehicles);

            public JsonGraphRootObject Graph { get; } =
                JsonConvert.DeserializeObject<JsonGraphRootObject>(data.copenhagen);

            public SimulationParameters Parameters;

            static SimResources()
            {
            }

            private SimResources()
            {
                Parameters = new SimulationParameters()
                {
                    SimulationIdentifier = Guid.NewGuid(),
                    RandomSeed = 42,
                    Graph = new GraphImporterService().GenerateGraph(Graph),
                    VehicleTemplate = Vehicle[0],
                    NumberOfVehicles = 1,
                    SimulationSpeed = 10
                };
            }

            public static SimResources Instance
            {
                get { return instance; }
            }
        }



        [TestMethod]
        public void TestLoadJsonResources()
        {
            var simResources = SimResources.Instance;
            Assert.AreEqual("Copenhagen", simResources.Graph.Name);
            Assert.AreEqual("Drone", simResources.Vehicle[0].Name);
        }

        [TestMethod]
        public void TestGraph()
        {
            var simpParameters = SimResources.Instance.Parameters;
            var jsonGraph = SimResources.Instance.Graph;
            
            foreach (var elem in jsonGraph.Vertices)
            {
                var testBase = elem.Name;
                var testDestintion = elem.Edges;
                foreach (var i in simpParameters.Graph)
                {
                    if (i.Info.Name == testBase)
                    {
                        // Check that the destinations listed int the json graph are the one added by the GraphImporter Service
                        var currentDest = i.Edges.Select(x => x.Destination.Info.Name).ToList();
                        CollectionAssert.AreEqual(testDestintion, currentDest, ToString());
                        // Check that each destination has an edge with the current base
                        foreach (var edge in i.Edges)
                        {
                            Console.WriteLine("Destination is = '" + edge.Destination.Info.Name + "' Base is = " + i.Info.Name); 
                            var otherDest = edge.Destination.Edges.Select(x => x.Destination.Info.Name).ToList();
                            foreach (var ii in otherDest)
                            {
                                Console.WriteLine(ii);
                            }
                            CollectionAssert.Contains(otherDest, i.Info.Name);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task  TestStartStopSimulation()
        {
            var simParameters = SimResources.Instance.Parameters;
            var simManager = new SimulationManager();
            simManager.AddSimulation(simParameters);
            var myres = simManager.StartSimulations();
            Assert.IsTrue(myres.IsCompleted);
             
            var stopRes = await simManager.StartSimulations();
            Assert.IsNull(stopRes.LastOrDefault());
        }
        
        [TestMethod]
        public async Task  TestAddMultipleSim()
        {
            var simParameters = SimResources.Instance.Parameters;
            var simManager = new SimulationManager();
            simManager.AddSimulation(simParameters);
            simManager.AddSimulation(simParameters);
            Assert.IsTrue(simManager.Simulations.Count == 2);
        }
    }
}
