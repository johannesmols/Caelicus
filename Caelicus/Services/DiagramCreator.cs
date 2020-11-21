using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Services
{
    public enum DataPointType
    {
        avgDeliveryTime,
        avgDeliveryCost,
        TimeVsCost
    }

    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public class DiagramCreator
    {
        public static Dictionary<DataPointType, BarConfig> GetDataSets(AppState appState)
        {
            Dictionary<DataPointType, BarConfig> dataSets = new Dictionary<DataPointType, BarConfig>();
            CreateConfigs(dataSets);

            SortedDictionary<int, Dictionary<string, Dictionary<DataPointType, double>>> DataPoints = new SortedDictionary<int, Dictionary<string, Dictionary<DataPointType, double>>>();

            foreach (var item in appState.SimulationHistories)
            {
                double totalDeliveryTime = 0;
                double totalDeliveryDistance = 0;
                double totalDistance = 0;
                double totalTime = 0;
                double totalCost = 0;

                double avgDeliveryTime = 0;
                double avgDeliveryDistance = 0;
                double avgDeliveryCost = 0;

                foreach (var order in item.Steps.Last().ClosedOrders)
                {
                    totalDeliveryTime += order.DeliveryTime ?? 0;
                    totalDeliveryDistance += order.DeliveryDistance ?? 0;
                }
                foreach (var veh in item.Vehicles)
                {
                    totalDistance += veh.TotalTravelDistance;
                    totalTime += veh.TotalTravelTime;
                }

                totalCost = item.Parameters.VehicleTemplate.PurchasingCost * item.Parameters.NumberOfVehicles
                            + totalDistance * item.Parameters.VehicleTemplate.CostPerKm
                            + totalTime * item.Parameters.VehicleTemplate.CostPerHour / 60 / 60;

                avgDeliveryTime = totalDeliveryTime / item.Parameters.NumberOfOrders;
                avgDeliveryCost = totalCost / item.Parameters.NumberOfOrders;

                if (!DataPoints.ContainsKey(item.Parameters.NumberOfVehicles))
                {
                    DataPoints.Add(item.Parameters.NumberOfVehicles, new Dictionary<string, Dictionary<DataPointType, double>>());                    
                }
                if(!DataPoints[item.Parameters.NumberOfVehicles].ContainsKey(item.Parameters.VehicleTemplate.Name))
                {
                    DataPoints[item.Parameters.NumberOfVehicles][item.Parameters.VehicleTemplate.Name] = new Dictionary<DataPointType, double>();
                }
                DataPoints[item.Parameters.NumberOfVehicles][item.Parameters.VehicleTemplate.Name][DataPointType.avgDeliveryTime] = avgDeliveryTime;
                DataPoints[item.Parameters.NumberOfVehicles][item.Parameters.VehicleTemplate.Name][DataPointType.avgDeliveryCost] = avgDeliveryCost;
            }

            var vehicle_types = new List<string>();
            foreach (var veh_number in DataPoints)
            {
                foreach (var veh in veh_number.Value)
                {
                    if (!vehicle_types.Contains(veh.Key))
                    {
                        vehicle_types.Add(veh.Key);
                        dataSets[DataPointType.avgDeliveryTime].Data.Labels.Add(veh.Key);
                        dataSets[DataPointType.avgDeliveryCost].Data.Labels.Add(veh.Key);
                    }
                }
            }

            Random r = new Random();
            foreach (var item in DataPoints)
            {
                BarDataset<double> dataSetAvgDelTime = new BarDataset<double>()
                {
                    BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255))),
                    Label = $"{item.Key} vehicle(s)"
                };
                BarDataset<double> dataSetAvgDeliveryCost = new BarDataset<double>()
                {
                    BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255))),
                    Label = $"{item.Key} vehicle(s)"
                };
                foreach (var veh in vehicle_types)
                {
                    if (item.Value.ContainsKey(veh))
                    {
                        dataSetAvgDelTime.Add(item.Value[veh][DataPointType.avgDeliveryTime]);
                        dataSetAvgDeliveryCost.Add(item.Value[veh][DataPointType.avgDeliveryCost]);
                    }
                    else
                    {
                        dataSetAvgDelTime.Add(0);
                        dataSetAvgDeliveryCost.Add(0);
                    }
                }
                dataSets[DataPointType.avgDeliveryTime].Data.Datasets.Add(dataSetAvgDelTime);
                dataSets[DataPointType.avgDeliveryCost].Data.Datasets.Add(dataSetAvgDeliveryCost);
            }
            return dataSets;
        }

        private static void CreateConfigs(Dictionary<DataPointType, BarConfig> dataSets)
        {
            dataSets[DataPointType.avgDeliveryTime] = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Position = Position.Bottom,
                        Display = true,
                    },
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Average Delivery Time"
                    },
                    Scales = new BarScales
                    {
                        YAxes = new List<CartesianAxis>()
                        {
                            new LinearCartesianAxis()
                            {
                                Ticks = new LinearCartesianTicks()
                                {
                                    BeginAtZero = true,
                                    Precision = 0
                                }
                            }
                        }
                    }
                }
            };
            dataSets[DataPointType.avgDeliveryCost] = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Position = Position.Bottom,
                        Display = true,
                    },
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Average Delivery Cost"
                    },
                    Scales = new BarScales
                    {
                        YAxes = new List<CartesianAxis>()
                        {
                            new LinearCartesianAxis()
                            {
                                Ticks = new LinearCartesianTicks()
                                {
                                    BeginAtZero = true,
                                    Precision = 0
                                }
                            }
                        }
                    }
                }
            };
            dataSets[DataPointType.TimeVsCost] = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Position = Position.Bottom,
                        Display = true,
                    },
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Devliver Time vs Cost"
                    },
                    Scales = new BarScales
                    {
                        YAxes = new List<CartesianAxis>()
                        {
                            new LinearCartesianAxis()
                            {
                                Ticks = new LinearCartesianTicks()
                                {
                                    BeginAtZero = true,
                                    Precision = 0
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
