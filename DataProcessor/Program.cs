using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronXL;
using Newtonsoft.Json;
using SimulationCore.Simulation.History;
using Syroot.Windows.IO;

namespace DataProcessor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var scenarios = new[] {"Scenario 1", "Scenario 3", "Inner City"};
            var path = Path.Combine(KnownFolders.Desktop.Path, "Caelicus", "final-final-results", "single orders");

            var excel = WorkBook.Create(ExcelFileFormat.XLSX);
            excel.Metadata.Title = "Caelicus Data Analysis";
            excel.Metadata.Author = "Caelicus";

            foreach (var scenario in scenarios)
            {
                var fullPath = Path.Combine(path, scenario);
                var allFiles = Directory.GetFiles(fullPath, "*.json", SearchOption.AllDirectories);

                var simHistories = new List<SimulationHistory>();
                foreach (var file in allFiles)
                {
                    simHistories.Add(JsonConvert.DeserializeObject<SimulationHistory>(await File.ReadAllTextAsync(file)));
                }

                await GenerateExcelSheet(simHistories, excel.CreateWorkSheet(scenario));
            }

            excel.SaveAs(Path.Combine(path, "analysis.xlsx"));
        }

        public static async Task GenerateExcelSheet(List<SimulationHistory> histories, WorkSheet worksheet)
        {
            var currentRow = 1;
            var currentCol = 'A';

            var groupedByName = histories.GroupBy(h => h.Parameters.VehicleTemplate.Name).OrderBy(g => g.Key);

            foreach (var group in groupedByName)
            {
                var simulationsForVehicle = group.ToList().OrderBy(s => s.Parameters.NumberOfVehicles).ToList();

                worksheet[$"{currentCol}{currentRow}"].Value = simulationsForVehicle.First().Parameters.VehicleTemplate.Name;
                worksheet[$"{(char) (currentCol + 1)}{currentRow}"].Value = "Orders per hour";
                worksheet[$"{(char) (currentCol + 2)}{currentRow}"].Value = "Cost per hour";
                worksheet[$"{(char) (currentCol + 3)}{currentRow}"].Value = "Fulfillment percentage";
                currentRow++;

                foreach (var simulation in simulationsForVehicle)
                {
                    var step = simulation.Steps?.LastOrDefault();
                    if (step is null) 
                        continue;

                    var totalTime = (double) step.SimulationStep;
                    var totalCost = step.ClosedOrders.Sum(o => o.DeliveryCost ?? 0 + o.PickupCost ?? 0);
                    var totalDeliveries = (double) step.ClosedOrders.Count;

                    var ordersPerHour = totalDeliveries /  totalTime * 60 * 60;
                    var costPerHour = totalCost / totalTime * 60 * 60;

                    var fulfillmentPercentage = ((double) step.ClosedOrders.Count) / ((double) (step.OpenOrders.Count + step.ClosedOrders.Count)) * 100d;

                    worksheet[$"{currentCol}{currentRow}"].Value = simulation.Parameters.NumberOfVehicles;
                    worksheet[$"{(char)(currentCol + 1)}{currentRow}"].Value = ordersPerHour;
                    worksheet[$"{(char)(currentCol + 2)}{currentRow}"].Value = costPerHour;
                    worksheet[$"{(char) (currentCol + 3)}{currentRow}"].Value = fulfillmentPercentage;
                    currentRow++;
                }
            }
        }
    }
}
