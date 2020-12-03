using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimulationCore.Helpers;
using SimulationCore.Models.Graph;
using SimulationCore.Models.Vehicles;
using SimulationCore.Services;
using SimulationCore.Simulation;
using SimulationCore.Simulation.History;
using Syroot.Windows.IO;

namespace SimulationCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            for (var s = 0; s < 4; s++)
            {
                for (var i = 1; i <= 64; i *= 2)
                {
                    Console.WriteLine($"Running scenario {s + 1} with {i} vehicles");
                    await Run(new Parameters(0, 64, Tuple.Create(0.1d, 5.5d), i, s).GetSimulationParameters(), $"scenario{s}_{i}vehicles_64orders");
                }
            }
        }

        public static async Task Run(List<SimulationParameters> parameters, string filename, bool onlyLastStep = true, bool saveCsv = true)
        {
            var simManager = new SimulationManager();
            var results = new List<SimulationHistory>();

            // Start simulations
            results.Clear();
            parameters.ForEach(simManager.AddSimulation);
            simManager.Simulations.ForEach(s => s.Item2.ProgressChanged += SimulationUpdate);
            results = await simManager.StartSimulations();

            // Create zip file
            var values = results.ToDictionary(
                result => result.Parameters.SimulationIdentifier.ToString(),
                result => JsonConvert.SerializeObject(
                    new SimulationHistory(result.Parameters)
                    {
                        Steps = onlyLastStep ? new List<SimulationHistoryStep>() { result.Steps.LastOrDefault() } : result.Steps
                    },
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            var archive = FileUtilities.CreateZipFile(values);

            // Save zip file
            var downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;
            var path = Path.Combine(downloadsPath, "results", $"{filename}.zip");

            Directory.CreateDirectory(Path.Combine(downloadsPath, "results"));

            Console.WriteLine("Downloading zip file to " + path);
            await File.WriteAllBytesAsync(path, archive);

            // Save csv file
            if (saveCsv)
            {
                Console.WriteLine("Append result to results.csv");
                var resCsv = Path.Combine(downloadsPath, "results", $"{filename}.csv");
                var writeHeader = !new FileInfo(resCsv).Exists;

                await using var sw = File.AppendText(resCsv);

                if (writeHeader)
                {
                    await sw.WriteLineAsync("Vehicle Type,Number of Vehicles,Number Of Orders,Delivery Time,Delivery Cost,Delivery Distance,Pickup Time,Pickup Cost,Pickup Distance");
                }

                var nfi = new NumberFormatInfo
                {
                    NumberDecimalSeparator = "."
                };

                foreach (var r in results)
                {
                    var lastStep = r.Steps.LastOrDefault();
                    if (lastStep != null)
                    {
                        var lastOrders = lastStep.ClosedOrders;
                        foreach (var order in lastOrders)
                        {
                            await sw.WriteLineAsync(
                                $"{r.Parameters.VehicleTemplate.Name},{r.Parameters.NumberOfVehicles},{r.Parameters.NumberOfOrders}," +
                                $"{order.DeliveryTime?.ToString(nfi)},{order.DeliveryCost?.ToString(nfi)},{order.DeliveryDistance?.ToString(nfi)}," +
                                $"{order.PickupTime?.ToString(nfi)},{order.PickupCost?.ToString(nfi)},{order.PickupDistance?.ToString(nfi)}");
                        }
                    }
                }
            }
        }

        public static void SimulationUpdate(object sender, SimulationProgress progress)
        {
            Console.WriteLine(progress.SimulationIdentifier + ": " + progress.Message);
        }
    }

    public class Parameters
    {
        // Adjustable Parameters
        public int RandomSeed;
        public int NumberOfVehicles;
        public int NumberOfOrders;
        public Tuple<double, double> MinMaxPayload;
        public int Scenario;

        public Parameters(int seed, int orders, Tuple<double, double> payload, int vehicles, int scenario)
        {
            RandomSeed = seed;
            NumberOfOrders = orders;
            NumberOfVehicles = vehicles;
            MinMaxPayload = payload;
            Scenario = scenario;
        }

        public List<SimulationParameters> GetSimulationParameters()
        {
            return new List<SimulationParameters>(
                JsonConvert.DeserializeObject<List<Vehicle>>(Vehicles).Select(v => new SimulationParameters()
                {
                    SimulationIdentifier = Guid.NewGuid(),
                    RandomSeed = RandomSeed,
                    JsonGraph = JsonConvert.DeserializeObject<JsonGraphRootObject>(Scenarios[Scenario]),
                    Graph = GraphImporterService.GenerateGraph(JsonConvert.DeserializeObject<JsonGraphRootObject>(Scenarios[Scenario])),
                    VehicleTemplate = v,
                    NumberOfVehicles = NumberOfVehicles,
                    SimulationSpeed = 0f,
                    NumberOfOrders = NumberOfOrders,
                    LogIntermediateSteps = false,
                    MinMaxPayload = MinMaxPayload
                }));
        }

        // Graphs
        public static readonly string[] Scenarios = {
            "{\"Name\":\"Scenario 1\",\"Vertices\":[{\"Name\":\"DTU\",\"Type\":\"base\",\"Latitude\":55.786327,\"Longitude\":12.523387,\"Edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15101,\"time\":2968},{\"travelMode\":\"Driving\",\"distance\":14215,\"time\":1319}]}]},{\"Name\":\"Tivoli\",\"Type\":\"target\",\"Latitude\":55.673452,\"Longitude\":12.567195,\"Edges\":[{\"target\":\"DTU\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15101,\"time\":2968},{\"travelMode\":\"Driving\",\"distance\":14215,\"time\":1319}]}]}]}",
            "{\"Name\":\"Scenario 2\",\"Vertices\":[{\"Name\":\"DTU\",\"Type\":\"base\",\"Latitude\":55.786327,\"Longitude\":12.523387,\"Edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15101,\"time\":2968},{\"travelMode\":\"Driving\",\"distance\":14215,\"time\":1319}]}]},{\"Name\":\"Tivoli\",\"Type\":\"target\",\"Latitude\":55.673452,\"Longitude\":12.567195,\"Edges\":[{\"target\":\"DTU\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14525,\"time\":3152},{\"travelMode\":\"Driving\",\"distance\":14713,\"time\":1403}]},{\"target\":\"Dragor\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15198,\"time\":3129},{\"travelMode\":\"Driving\",\"distance\":17418,\"time\":1497}]},{\"target\":\"Glostrup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":11498,\"time\":2770},{\"travelMode\":\"Driving\",\"distance\":15813,\"time\":1439}]}]},{\"name\":\"Dragor\",\"type\":\"target\",\"latitude\":55.594293,\"longitude\":12.674374,\"edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15858,\"time\":3177},{\"travelMode\":\"Driving\",\"distance\":17876,\"time\":1632}]}]},{\"name\":\"Koge\",\"type\":\"target\",\"latitude\":55.455554,\"longitude\":12.183992,\"edges\":[{\"target\":\"Glostrup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":30204,\"time\":5833},{\"travelMode\":\"Driving\",\"distance\":36450,\"time\":1919}]}]},{\"name\":\"Glostrup\",\"type\":\"base\",\"latitude\":55.663069,\"longitude\":12.397718,\"edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":11528,\"time\":2632},{\"travelMode\":\"Driving\",\"distance\":11109,\"time\":1520}]},{\"target\":\"Koge\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":29936,\"time\":5715},{\"travelMode\":\"Driving\",\"distance\":39696,\"time\":1957}]}]}]}",
            "{\"Name\":\"Scenario 3\",\"Vertices\":[{\"Name\":\"Gilleleje\",\"Type\":\"base\",\"Latitude\":56.126063875057895,\"Longitude\":12.296310150330507,\"Edges\":[{\"target\":\"Rageleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":9432,\"time\":1696},{\"travelMode\":\"Driving\",\"distance\":9435,\"time\":749}]},{\"target\":\"Ramlose\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":21762,\"time\":4002},{\"travelMode\":\"Driving\",\"distance\":21231,\"time\":1289}]},{\"target\":\"Anisse\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":22307,\"time\":4051},{\"travelMode\":\"Driving\",\"distance\":23829,\"time\":1562}]},{\"target\":\"Kagerup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":16283,\"time\":2909},{\"travelMode\":\"Driving\",\"distance\":16175,\"time\":1041}]},{\"target\":\"Dronningsmolle\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":7011,\"time\":1359},{\"travelMode\":\"Driving\",\"distance\":6886,\"time\":598}]},{\"target\":\"Esrum\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14142,\"time\":2634},{\"travelMode\":\"Driving\",\"distance\":11562,\"time\":866}]}]},{\"Name\":\"Rageleje\",\"Type\":\"target\",\"Latitude\":56.09966535810104,\"Longitude\":12.162931275991793,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":10920,\"time\":2081},{\"travelMode\":\"Driving\",\"distance\":9435,\"time\":748}]}]},{\"Name\":\"Ramlose\",\"Type\":\"target\",\"Latitude\":56.014551949784526,\"Longitude\":12.111542878519302,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":22854,\"time\":4150},{\"travelMode\":\"Driving\",\"distance\":21777,\"time\":1333}]}]},{\"Name\":\"Anisse\",\"Type\":\"target\",\"Latitude\":55.97935561674914,\"Longitude\":12.172169639582355,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":23448,\"time\":4347},{\"travelMode\":\"Driving\",\"distance\":23855,\"time\":1589}]}]},{\"Name\":\"Kagerup\",\"Type\":\"target\",\"Latitude\":55.999379479700124,\"Longitude\":12.261666286865907,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":17141,\"time\":3101},{\"travelMode\":\"Driving\",\"distance\":16189,\"time\":1044}]}]},{\"Name\":\"Dronningsmolle\",\"Type\":\"target\",\"Latitude\":56.09966535810104,\"Longitude\":12.380032820369962,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":6606,\"time\":1304},{\"travelMode\":\"Driving\",\"distance\":7045,\"time\":608}]}]},{\"Name\":\"Esrum\",\"Type\":\"target\",\"Latitude\":56.05036115734118,\"Longitude\":12.374258843125862,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":13738,\"time\":2521},{\"travelMode\":\"Driving\",\"distance\":11756,\"time\":875}]}]}]}",
            "{\"name\":\"Scenario 4\",\"vertices\":[{\"name\":\"DTU\",\"type\":\"base\",\"latitude\":55.786327,\"longitude\":12.523387,\"edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15101,\"time\":2968},{\"travelMode\":\"Driving\",\"distance\":14215,\"time\":1319}]},{\"target\":\"Amager Strandpark\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":20514,\"time\":3780},{\"travelMode\":\"Driving\",\"distance\":39688,\"time\":1972}]},{\"target\":\"Ballerup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14582,\"time\":2798},{\"travelMode\":\"Driving\",\"distance\":15506,\"time\":1243}]}]},{\"name\":\"Glostrup\",\"type\":\"base\",\"latitude\":55.663069,\"longitude\":12.397718,\"edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":11528,\"time\":2632},{\"travelMode\":\"Driving\",\"distance\":11109,\"time\":1520}]},{\"target\":\"Roskilde\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":20872,\"time\":4063},{\"travelMode\":\"Driving\",\"distance\":21770,\"time\":1621}]},{\"target\":\"Ballerup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14152,\"time\":2822},{\"travelMode\":\"Driving\",\"distance\":10888,\"time\":986}]},{\"target\":\"Brondby Strand\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":5947,\"time\":1247},{\"travelMode\":\"Driving\",\"distance\":6643,\"time\":725}]}]},{\"name\":\"Tivoli\",\"type\":\"target\",\"latitude\":55.673452,\"longitude\":12.567195,\"edges\":[{\"target\":\"DTU\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14525,\"time\":3152},{\"travelMode\":\"Driving\",\"distance\":14713,\"time\":1403}]},{\"target\":\"Dragor\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15198,\"time\":3129},{\"travelMode\":\"Driving\",\"distance\":17418,\"time\":1497}]},{\"target\":\"Brondby Strand\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":13244,\"time\":2760},{\"travelMode\":\"Driving\",\"distance\":16703,\"time\":1236}]},{\"target\":\"Glostrup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":11498,\"time\":2770},{\"travelMode\":\"Driving\",\"distance\":15813,\"time\":1439}]}]},{\"name\":\"Amager Strandpark\",\"type\":\"target\",\"latitude\":55.655827,\"longitude\":12.647102,\"edges\":[{\"target\":\"DTU\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":19908,\"time\":3858},{\"travelMode\":\"Driving\",\"distance\":40397,\"time\":1970}]},{\"target\":\"Dragor\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":9678,\"time\":1765},{\"travelMode\":\"Driving\",\"distance\":9975,\"time\":1072}]}]},{\"name\":\"Dragor\",\"type\":\"target\",\"latitude\":55.594293,\"longitude\":12.674374,\"edges\":[{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":15858,\"time\":3177},{\"travelMode\":\"Driving\",\"distance\":17867,\"time\":1632}]},{\"target\":\"Amager Strandpark\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":9692,\"time\":1825},{\"travelMode\":\"Driving\",\"distance\":10722,\"time\":1170}]},{\"target\":\"Brondby Strand\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":19616,\"time\":3804},{\"travelMode\":\"Driving\",\"distance\":21605,\"time\":1565}]}]},{\"name\":\"Brondby Strand\",\"type\":\"target\",\"latitude\":55.620991,\"longitude\":12.421854,\"edges\":[{\"target\":\"Dragor\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":19106,\"time\":3618},{\"travelMode\":\"Driving\",\"distance\":20893,\"time\":1556}]},{\"target\":\"Tivoli\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":12909,\"time\":2550},{\"travelMode\":\"Driving\",\"distance\":16766,\"time\":1385}]},{\"target\":\"Koge\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":26485,\"time\":5053},{\"travelMode\":\"Driving\",\"distance\":33960,\"time\":1818}]},{\"target\":\"Glostrup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":6204,\"time\":1333},{\"travelMode\":\"Driving\",\"distance\":6806,\"time\":840}]},{\"target\":\"Roskilde\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":26044,\"time\":4933},{\"travelMode\":\"Driving\",\"distance\":25723,\"time\":1744}]}]},{\"name\":\"Koge\",\"type\":\"target\",\"latitude\":55.455554,\"longitude\":12.183992,\"edges\":[{\"target\":\"Brondby Strand\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":26969,\"time\":5144},{\"travelMode\":\"Driving\",\"distance\":33723,\"time\":1676}]},{\"target\":\"Roskilde\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":26049,\"time\":5096},{\"travelMode\":\"Driving\",\"distance\":25514,\"time\":2086}]}]},{\"name\":\"Roskilde\",\"type\":\"target\",\"latitude\":55.642504,\"longitude\":12.080403,\"edges\":[{\"target\":\"Koge\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":25755,\"time\":4856},{\"travelMode\":\"Driving\",\"distance\":26417,\"time\":2110}]},{\"target\":\"Brondby Strand\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":25258,\"time\":4662},{\"travelMode\":\"Driving\",\"distance\":28428,\"time\":1688}]},{\"target\":\"Ballerup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":26246,\"time\":5059},{\"travelMode\":\"Driving\",\"distance\":27360,\"time\":1603}]},{\"target\":\"Glostrup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":20625,\"time\":3914},{\"travelMode\":\"Driving\",\"distance\":23907,\"time\":1670}]}]},{\"name\":\"Ballerup\",\"type\":\"target\",\"latitude\":55.730093,\"longitude\":12.358226,\"edges\":[{\"target\":\"DTU\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14796,\"time\":2797},{\"travelMode\":\"Driving\",\"distance\":16703,\"time\":1304}]},{\"target\":\"Roskilde\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":27294,\"time\":5153},{\"travelMode\":\"Driving\",\"distance\":24964,\"time\":1473}]},{\"target\":\"Glostrup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":11925,\"time\":2346},{\"travelMode\":\"Driving\",\"distance\":10823,\"time\":936}]}]}]}"
        };

        // Vehicles
        public const string Vehicles = "[{\"name\":\"DJI Matrice 600 Drone\",\"averageSpeed\":64,\"fuelCapacity\":37331,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0.18,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":5.5,\"purchasingCost\":42443,\"costPerHour\":15.88,\"costPerKm\":0,\"travelMode\":\"Transit\"},{\"name\":\"Quaternium Hybrix 2.1 Drone\",\"averageSpeed\":50,\"fuelCapacity\":200000,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0.1,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":10,\"purchasingCost\":149022,\"costPerHour\":59.53,\"costPerKm\":0,\"travelMode\":\"Transit\"},{\"name\":\"Mercedes Actros 1845 LS\",\"averageSpeed\":80,\"fuelCapacity\":400,\"baseFuelConsumption\":0.000205,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":900,\"allowRefuelAtTarget\":false,\"maxPayload\":33000,\"purchasingCost\":350235,\"costPerHour\":720,\"costPerKm\":13,\"travelMode\":\"Driving\"},{\"name\":\"Mercedes Sprinter 311 CDI Standard\",\"averageSpeed\":110,\"fuelCapacity\":65,\"baseFuelConsumption\":0.000082,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":1291,\"purchasingCost\":318677,\"costPerHour\":340,\"costPerKm\":10.6,\"travelMode\":\"Driving\"},{\"name\":\"City Bike\",\"averageSpeed\":20,\"fuelCapacity\":99999999,\"baseFuelConsumption\":0,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":0,\"allowRefuelAtTarget\":false,\"maxPayload\":40,\"purchasingCost\":5999,\"costPerHour\":130,\"costPerKm\":0,\"travelMode\":\"Bicycling\"},{\"name\":\"Seaside Bike\",\"averageSpeed\":25,\"fuelCapacity\":65000,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0.005,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":180,\"purchasingCost\":33995,\"costPerHour\":130,\"costPerKm\":0,\"travelMode\":\"Bicycling\"}]";
    }
}