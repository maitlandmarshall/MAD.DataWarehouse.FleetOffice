using Microsoft.Extensions.Hosting;
using MIFCore;
using MIFCore.Hangfire.Analytics;
using MIFCore.Http;
using System;

namespace MAD.DataWarehouse.FleetOffice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            IntegrationHost.CreateDefaultBuilder(args)
                .UseAspNetCore()
                .UseAppInsights()
                .UseStartup<Startup>();
    }
}
