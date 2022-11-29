using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIFCore.Common;
using MIFCore.Hangfire.APIETL.Extract;
using MIFCore.Settings;
using OAuthB0ner;
using System;
using System.Threading.Tasks;

namespace MAD.DataWarehouse.FleetOffice
{
    internal class Startup
    {
        private readonly IConfiguration configuration;

        public Startup()
        {
            this.configuration = Globals.DefaultConfiguration;
        }

        public void ConfigureServices(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddIntegrationSettings<AppConfig>();

            serviceDescriptors.AddTransient<AuthenticationDelegatingHandler>();

            serviceDescriptors
                .AddHttpClient("fleet", cfg =>
                {
                    cfg.BaseAddress = new Uri("https://thefleetoffice.net.au:7080");
                })
                .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

            serviceDescriptors.AddEndpointsApiExplorer();

            serviceDescriptors.AddOAuthB0ner(oauth =>
            {
                this.configuration.Bind("oauth", oauth);
            });
        }

        public async Task Configure(IApiEndpointRegister apiEndpointRegister)
        {
            await apiEndpointRegister.Register();
        }
    }
}