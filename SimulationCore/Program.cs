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
            for (var s = 0; s < 3; s++)
            {
                for (var i = 1; i <= 64; i *= 2)
                {
                    Console.WriteLine($"Running scenario {s + 1} with {i} vehicles");
                    await Run(new Parameters(0, 100, Tuple.Create(0.1d, 5.5d), i, s).GetSimulationParameters(), $"scenario{s}_{i}vehicles_100orders_multipleorders");
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
            "{\"Name\":\"Scenario 1\",\"Vertices\":[{\"Name\":\"DTU\",\"Type\":\"base\",\"Latitude\":55.786327,\"Longitude\":12.523387,\"Edges\":[{\"Target\":\"Tivoli\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":15101,\"Time\":2965},{\"TravelMode\":\"Driving\",\"Distance\":14215,\"Time\":1322},{\"TravelMode\":\"Transit\",\"Distance\":15224,\"Time\":2967},{\"TravelMode\":\"Walking\",\"Distance\":14158,\"Time\":10534}]}]},{\"Name\":\"Tivoli\",\"Type\":\"target\",\"Latitude\":55.673452,\"Longitude\":12.567195,\"Edges\":[{\"Target\":\"DTU\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":14525,\"Time\":3150},{\"TravelMode\":\"Driving\",\"Distance\":14713,\"Time\":1401},{\"TravelMode\":\"Transit\",\"Distance\":17453,\"Time\":3165},{\"TravelMode\":\"Walking\",\"Distance\":14158,\"Time\":10706}]}]}]}",
            "{\"Name\":\"Scenario 3\",\"Vertices\":[{\"Name\":\"Gilleleje\",\"Type\":\"base\",\"Latitude\":56.126063875057895,\"Longitude\":12.296310150330507,\"Edges\":[{\"target\":\"Rageleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":9432,\"time\":1696},{\"travelMode\":\"Driving\",\"distance\":9435,\"time\":749}]},{\"target\":\"Ramlose\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":21762,\"time\":4002},{\"travelMode\":\"Driving\",\"distance\":21231,\"time\":1289}]},{\"target\":\"Anisse\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":22307,\"time\":4051},{\"travelMode\":\"Driving\",\"distance\":23829,\"time\":1562}]},{\"target\":\"Kagerup\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":16283,\"time\":2909},{\"travelMode\":\"Driving\",\"distance\":16175,\"time\":1041}]},{\"target\":\"Dronningsmolle\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":7011,\"time\":1359},{\"travelMode\":\"Driving\",\"distance\":6886,\"time\":598}]},{\"target\":\"Esrum\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":14142,\"time\":2634},{\"travelMode\":\"Driving\",\"distance\":11562,\"time\":866}]}]},{\"Name\":\"Rageleje\",\"Type\":\"target\",\"Latitude\":56.09966535810104,\"Longitude\":12.162931275991793,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":10920,\"time\":2081},{\"travelMode\":\"Driving\",\"distance\":9435,\"time\":748}]}]},{\"Name\":\"Ramlose\",\"Type\":\"target\",\"Latitude\":56.014551949784526,\"Longitude\":12.111542878519302,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":22854,\"time\":4150},{\"travelMode\":\"Driving\",\"distance\":21777,\"time\":1333}]}]},{\"Name\":\"Anisse\",\"Type\":\"target\",\"Latitude\":55.97935561674914,\"Longitude\":12.172169639582355,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":23448,\"time\":4347},{\"travelMode\":\"Driving\",\"distance\":23855,\"time\":1589}]}]},{\"Name\":\"Kagerup\",\"Type\":\"target\",\"Latitude\":55.999379479700124,\"Longitude\":12.261666286865907,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":17141,\"time\":3101},{\"travelMode\":\"Driving\",\"distance\":16189,\"time\":1044}]}]},{\"Name\":\"Dronningsmolle\",\"Type\":\"target\",\"Latitude\":56.09966535810104,\"Longitude\":12.380032820369962,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":6606,\"time\":1304},{\"travelMode\":\"Driving\",\"distance\":7045,\"time\":608}]}]},{\"Name\":\"Esrum\",\"Type\":\"target\",\"Latitude\":56.05036115734118,\"Longitude\":12.374258843125862,\"Edges\":[{\"target\":\"Gilleleje\",\"modes\":[{\"travelMode\":\"Bicycling\",\"distance\":13738,\"time\":2521},{\"travelMode\":\"Driving\",\"distance\":11756,\"time\":875}]}]}]}",
            "{\"Name\":\"Inner City\",\"Vertices\":[{\"Name\":\"Rigshospitalet\",\"Type\":\"both\",\"Latitude\":55.696581631707076,\"Longitude\":12.566857345253906,\"Edges\":[{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":4885,\"Time\":976},{\"TravelMode\":\"Driving\",\"Distance\":4449,\"Time\":866},{\"TravelMode\":\"Transit\",\"Distance\":5140,\"Time\":1914},{\"TravelMode\":\"Walking\",\"Distance\":3846,\"Time\":2915}]},{\"Target\":\"Bispebjerg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":3254,\"Time\":666},{\"TravelMode\":\"Driving\",\"Distance\":3876,\"Time\":759},{\"TravelMode\":\"Transit\",\"Distance\":3271,\"Time\":1292},{\"TravelMode\":\"Walking\",\"Distance\":3203,\"Time\":2417}]},{\"Target\":\"Amager Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7727,\"Time\":1603},{\"TravelMode\":\"Driving\",\"Distance\":7610,\"Time\":1223},{\"TravelMode\":\"Transit\",\"Distance\":10818,\"Time\":3046},{\"TravelMode\":\"Walking\",\"Distance\":6668,\"Time\":4952}]},{\"Target\":\"Psykiatrisk Akutmodtagelse\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7774,\"Time\":1491},{\"TravelMode\":\"Driving\",\"Distance\":8335,\"Time\":1150},{\"TravelMode\":\"Transit\",\"Distance\":9239,\"Time\":2317},{\"TravelMode\":\"Walking\",\"Distance\":7136,\"Time\":5312}]},{\"Target\":\"Gentofte Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":5243,\"Time\":1108},{\"TravelMode\":\"Driving\",\"Distance\":6347,\"Time\":783},{\"TravelMode\":\"Transit\",\"Distance\":6637,\"Time\":1558},{\"TravelMode\":\"Walking\",\"Distance\":5210,\"Time\":3930}]}]},{\"Name\":\"Frederiksberg Hospital\",\"Type\":\"both\",\"Latitude\":55.68524758990225,\"Longitude\":12.524759919046602,\"Edges\":[{\"Target\":\"Rigshospitalet\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":4300,\"Time\":866},{\"TravelMode\":\"Driving\",\"Distance\":4877,\"Time\":943},{\"TravelMode\":\"Transit\",\"Distance\":6292,\"Time\":1798},{\"TravelMode\":\"Walking\",\"Distance\":3846,\"Time\":2896}]},{\"Target\":\"Bispebjerg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":3774,\"Time\":778},{\"TravelMode\":\"Driving\",\"Distance\":4345,\"Time\":873},{\"TravelMode\":\"Transit\",\"Distance\":4714,\"Time\":1363},{\"TravelMode\":\"Walking\",\"Distance\":3626,\"Time\":2716}]},{\"Target\":\"Amager Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8112,\"Time\":1755},{\"TravelMode\":\"Driving\",\"Distance\":8695,\"Time\":1400},{\"TravelMode\":\"Transit\",\"Distance\":8408,\"Time\":1766},{\"TravelMode\":\"Walking\",\"Distance\":7784,\"Time\":5789}]},{\"Target\":\"Herlev Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":9146,\"Time\":1829},{\"TravelMode\":\"Driving\",\"Distance\":9629,\"Time\":1020},{\"TravelMode\":\"Transit\",\"Distance\":12176,\"Time\":3811},{\"TravelMode\":\"Walking\",\"Distance\":8120,\"Time\":6081}]},{\"Target\":\"Psykiatrisk Akutmodtagelse\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8262,\"Time\":1679},{\"TravelMode\":\"Driving\",\"Distance\":9420,\"Time\":1326},{\"TravelMode\":\"Transit\",\"Distance\":8756,\"Time\":1373},{\"TravelMode\":\"Walking\",\"Distance\":7705,\"Time\":5673}]},{\"Target\":\"Hvidovre Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7748,\"Time\":1451},{\"TravelMode\":\"Driving\",\"Distance\":9390,\"Time\":1206},{\"TravelMode\":\"Transit\",\"Distance\":7637,\"Time\":2300},{\"TravelMode\":\"Walking\",\"Distance\":6554,\"Time\":4902}]},{\"Target\":\"Rigshospitalet Glostrup\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":10860,\"Time\":2153},{\"TravelMode\":\"Driving\",\"Distance\":11721,\"Time\":1462},{\"TravelMode\":\"Transit\",\"Distance\":19906,\"Time\":4569},{\"TravelMode\":\"Walking\",\"Distance\":9715,\"Time\":7402}]}]},{\"Name\":\"Bispebjerg Hospital\",\"Type\":\"both\",\"Latitude\":55.714141100260704,\"Longitude\":12.540088383339548,\"Edges\":[{\"Target\":\"Rigshospitalet\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":3282,\"Time\":690},{\"TravelMode\":\"Driving\",\"Distance\":3776,\"Time\":776},{\"TravelMode\":\"Transit\",\"Distance\":3439,\"Time\":1358},{\"TravelMode\":\"Walking\",\"Distance\":3203,\"Time\":2354}]},{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":3775,\"Time\":715},{\"TravelMode\":\"Driving\",\"Distance\":3719,\"Time\":792},{\"TravelMode\":\"Transit\",\"Distance\":4522,\"Time\":1346},{\"TravelMode\":\"Walking\",\"Distance\":3618,\"Time\":2667}]},{\"Target\":\"Herlev Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8431,\"Time\":1618},{\"TravelMode\":\"Driving\",\"Distance\":9061,\"Time\":981},{\"TravelMode\":\"Transit\",\"Distance\":8689,\"Time\":2269},{\"TravelMode\":\"Walking\",\"Distance\":7624,\"Time\":5672}]},{\"Target\":\"Gentofte Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":3856,\"Time\":879},{\"TravelMode\":\"Driving\",\"Distance\":4274,\"Time\":689},{\"TravelMode\":\"Transit\",\"Distance\":4127,\"Time\":1354},{\"TravelMode\":\"Walking\",\"Distance\":3503,\"Time\":2664}]}]},{\"Name\":\"Amager Hospital\",\"Type\":\"both\",\"Latitude\":55.65525095150975,\"Longitude\":12.620545109049955,\"Edges\":[{\"Target\":\"Rigshospitalet\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7024,\"Time\":1431},{\"TravelMode\":\"Driving\",\"Distance\":7961,\"Time\":1292},{\"TravelMode\":\"Transit\",\"Distance\":7440,\"Time\":2224},{\"TravelMode\":\"Walking\",\"Distance\":6665,\"Time\":4983}]},{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8498,\"Time\":1826},{\"TravelMode\":\"Driving\",\"Distance\":8335,\"Time\":1375},{\"TravelMode\":\"Transit\",\"Distance\":8664,\"Time\":1731},{\"TravelMode\":\"Walking\",\"Distance\":7783,\"Time\":5833}]},{\"Target\":\"Psykiatrisk Akutmodtagelse\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":4163,\"Time\":843},{\"TravelMode\":\"Driving\",\"Distance\":3725,\"Time\":619},{\"TravelMode\":\"Transit\",\"Distance\":6592,\"Time\":1653},{\"TravelMode\":\"Walking\",\"Distance\":2999,\"Time\":2216}]}]},{\"Name\":\"Herlev Hospital\",\"Type\":\"both\",\"Latitude\":55.73088,\"Longitude\":12.443262,\"Edges\":[{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":9087,\"Time\":1659},{\"TravelMode\":\"Driving\",\"Distance\":9053,\"Time\":909},{\"TravelMode\":\"Transit\",\"Distance\":9028,\"Time\":2840},{\"TravelMode\":\"Walking\",\"Distance\":8120,\"Time\":6000}]},{\"Target\":\"Bispebjerg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8279,\"Time\":1529},{\"TravelMode\":\"Driving\",\"Distance\":8242,\"Time\":949},{\"TravelMode\":\"Transit\",\"Distance\":9489,\"Time\":2347},{\"TravelMode\":\"Walking\",\"Distance\":7623,\"Time\":5636}]},{\"Target\":\"Gentofte Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7221,\"Time\":1468},{\"TravelMode\":\"Driving\",\"Distance\":10256,\"Time\":789},{\"TravelMode\":\"Transit\",\"Distance\":8465,\"Time\":2284},{\"TravelMode\":\"Walking\",\"Distance\":7215,\"Time\":5398}]},{\"Target\":\"Rigshospitalet Glostrup\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8104,\"Time\":1544},{\"TravelMode\":\"Driving\",\"Distance\":11288,\"Time\":871},{\"TravelMode\":\"Transit\",\"Distance\":8341,\"Time\":1240},{\"TravelMode\":\"Walking\",\"Distance\":8104,\"Time\":6010}]}]},{\"Name\":\"Gentofte Hospital\",\"Type\":\"both\",\"Latitude\":55.738999,\"Longitude\":12.547113,\"Edges\":[{\"Target\":\"Herlev Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7320,\"Time\":1480},{\"TravelMode\":\"Driving\",\"Distance\":10038,\"Time\":798},{\"TravelMode\":\"Transit\",\"Distance\":8497,\"Time\":2590},{\"TravelMode\":\"Walking\",\"Distance\":7216,\"Time\":5416}]},{\"Target\":\"Bispebjerg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":3843,\"Time\":798},{\"TravelMode\":\"Driving\",\"Distance\":4182,\"Time\":665},{\"TravelMode\":\"Transit\",\"Distance\":4232,\"Time\":1412},{\"TravelMode\":\"Walking\",\"Distance\":3491,\"Time\":2639}]},{\"Target\":\"Rigshospitalet\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":6548,\"Time\":1195},{\"TravelMode\":\"Driving\",\"Distance\":6382,\"Time\":809},{\"TravelMode\":\"Transit\",\"Distance\":6400,\"Time\":1610},{\"TravelMode\":\"Walking\",\"Distance\":5273,\"Time\":3911}]}]},{\"Name\":\"Psykiatrisk Akutmodtagelse\",\"Type\":\"both\",\"Latitude\":55.644461,\"Longitude\":12.586503,\"Edges\":[{\"Target\":\"Amager Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":4155,\"Time\":887},{\"TravelMode\":\"Driving\",\"Distance\":3518,\"Time\":598},{\"TravelMode\":\"Transit\",\"Distance\":4240,\"Time\":1045},{\"TravelMode\":\"Walking\",\"Distance\":2999,\"Time\":2227}]},{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8379,\"Time\":1678},{\"TravelMode\":\"Driving\",\"Distance\":8670,\"Time\":1264},{\"TravelMode\":\"Transit\",\"Distance\":8756,\"Time\":1414},{\"TravelMode\":\"Walking\",\"Distance\":7699,\"Time\":5706}]},{\"Target\":\"Rigshospitalet\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7815,\"Time\":1502},{\"TravelMode\":\"Driving\",\"Distance\":8296,\"Time\":1181},{\"TravelMode\":\"Transit\",\"Distance\":7936,\"Time\":1882},{\"TravelMode\":\"Walking\",\"Distance\":7128,\"Time\":5333}]},{\"Target\":\"Hvidovre Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":9370,\"Time\":1886},{\"TravelMode\":\"Driving\",\"Distance\":12866,\"Time\":1046},{\"TravelMode\":\"Transit\",\"Distance\":12399,\"Time\":3024},{\"TravelMode\":\"Walking\",\"Distance\":8535,\"Time\":6428}]}]},{\"Name\":\"Hvidovre Hospital\",\"Type\":\"both\",\"Latitude\":55.648609,\"Longitude\":12.470186,\"Edges\":[{\"Target\":\"Psykiatrisk Akutmodtagelse\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8579,\"Time\":1735},{\"TravelMode\":\"Driving\",\"Distance\":13100,\"Time\":1075},{\"TravelMode\":\"Transit\",\"Distance\":16734,\"Time\":2928},{\"TravelMode\":\"Walking\",\"Distance\":8582,\"Time\":6394}]},{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":7421,\"Time\":1456},{\"TravelMode\":\"Driving\",\"Distance\":9265,\"Time\":1213},{\"TravelMode\":\"Transit\",\"Distance\":7558,\"Time\":2146},{\"TravelMode\":\"Walking\",\"Distance\":6554,\"Time\":4914}]},{\"Target\":\"Rigshospitalet Glostrup\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8890,\"Time\":1817},{\"TravelMode\":\"Driving\",\"Distance\":9308,\"Time\":969},{\"TravelMode\":\"Transit\",\"Distance\":11671,\"Time\":2410},{\"TravelMode\":\"Walking\",\"Distance\":7227,\"Time\":5493}]}]},{\"Name\":\"Rigshospitalet Glostrup\",\"Type\":\"both\",\"Latitude\":55.669843,\"Longitude\":12.389111,\"Edges\":[{\"Target\":\"Hvidovre Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8403,\"Time\":1604},{\"TravelMode\":\"Driving\",\"Distance\":9736,\"Time\":1009},{\"TravelMode\":\"Transit\",\"Distance\":11156,\"Time\":2625},{\"TravelMode\":\"Walking\",\"Distance\":7246,\"Time\":5429}]},{\"Target\":\"Herlev Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":8609,\"Time\":1640},{\"TravelMode\":\"Driving\",\"Distance\":8609,\"Time\":998},{\"TravelMode\":\"Transit\",\"Distance\":8270,\"Time\":1402},{\"TravelMode\":\"Walking\",\"Distance\":8104,\"Time\":6053}]},{\"Target\":\"Frederiksberg Hospital\",\"Modes\":[{\"TravelMode\":\"Bicycling\",\"Distance\":10931,\"Time\":2121},{\"TravelMode\":\"Driving\",\"Distance\":11736,\"Time\":1382},{\"TravelMode\":\"Transit\",\"Distance\":15221,\"Time\":3584},{\"TravelMode\":\"Walking\",\"Distance\":9715,\"Time\":7348}]}]}]}"
        };

        // Vehicles
        public const string Vehicles = "[{\"name\":\"DJI Matrice 600 Drone\",\"averageSpeed\":40,\"fuelCapacity\":37331,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0.18,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":5.5,\"purchasingCost\":42443,\"costPerHour\":15.88,\"costPerKm\":0,\"travelMode\":\"Transit\"},{\"name\":\"Quaternium Hybrix 2.1 Drone\",\"averageSpeed\":50,\"fuelCapacity\":200000,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0.1,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":10,\"purchasingCost\":149022,\"costPerHour\":59.53,\"costPerKm\":0,\"travelMode\":\"Transit\"},{\"name\":\"Mercedes Actros 1845 LS\",\"averageSpeed\":80,\"fuelCapacity\":400,\"baseFuelConsumption\":0.000205,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":900,\"allowRefuelAtTarget\":false,\"maxPayload\":33000,\"purchasingCost\":350235,\"costPerHour\":720,\"costPerKm\":13,\"travelMode\":\"Driving\"},{\"name\":\"Mercedes Sprinter 311 CDI Standard\",\"averageSpeed\":110,\"fuelCapacity\":65,\"baseFuelConsumption\":0.000082,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":1291,\"purchasingCost\":318677,\"costPerHour\":340,\"costPerKm\":10.6,\"travelMode\":\"Driving\"},{\"name\":\"City Bike\",\"averageSpeed\":20,\"fuelCapacity\":99999999,\"baseFuelConsumption\":0,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":0,\"allowRefuelAtTarget\":false,\"maxPayload\":40,\"purchasingCost\":5999,\"costPerHour\":130,\"costPerKm\":0,\"travelMode\":\"Bicycling\"},{\"name\":\"Seaside Bike\",\"averageSpeed\":25,\"fuelCapacity\":65000,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0.005,\"refuelingTime\":300,\"allowRefuelAtTarget\":false,\"maxPayload\":180,\"purchasingCost\":33995,\"costPerHour\":130,\"costPerKm\":0,\"travelMode\":\"Bicycling\"},{\"name\":\"Lockheed SR-71 Blackbird\",\"averageSpeed\":3529,\"fuelCapacity\":999999999999,\"baseFuelConsumption\":1,\"extraFuelConsumptionPerKg\":0,\"refuelingTime\":30,\"allowRefuelAtTarget\":false,\"maxPayload\":25,\"purchasingCost\":50000000000,\"costPerHour\":1000000,\"costPerKm\":1,\"travelMode\":\"Transit\"}]";
    }
}