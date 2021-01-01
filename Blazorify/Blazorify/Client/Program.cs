using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace Blazorify.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddLogging(options =>
            {
                options.AddBrowserConsole();
                options.SetMinimumLevel(LogLevel.Information);
            });

            await builder.Build().RunAsync();
        }
    }
}
