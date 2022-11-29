using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIFCore.Common;
using MIFCore.Hangfire.APIETL;
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
                .AddHttpClient(string.Empty, cfg =>
                {
                    cfg.BaseAddress = new Uri("https://thefleetoffice.net.au:7080");
                })
                .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

            serviceDescriptors.AddApiEndpointsToExtract();
            serviceDescriptors.AddOAuthB0ner(oauth =>
            {
                this.configuration.Bind("oauth", oauth);
            });
        }

        public async Task PostConfigure(IApiEndpointRegister apiEndpointRegister)
        {
            await apiEndpointRegister.Register();
        }
    }
}