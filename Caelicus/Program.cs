using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorApp.Services;
using Blazored.LocalStorage;
using GoogleMapsComponents;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimulationCore.Services;

namespace BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<AppState>();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBlazorGoogleMaps();
            builder.Services.AddTransient<TimerService>();

            await builder.Build().RunAsync();
        }
    }
}
