using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Modularity;

namespace Vls.Abp.Examples.Hubs
{
    public class NatsHubOptions
    {
        public Assembly Assembly { get; set; }
    }

    [DependsOn(typeof(AbpNatsMqModule))]
    public class AbpNatsHubsModule : AbpModule
    {
        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            var serviceProvider = context.Services.BuildServiceProvider();
            var applicationServices = serviceProvider.GetServices<IApplicationService>();
            foreach (var app in applicationServices)
            {
                var implementation = app.GetType();
                var contracts = implementation.GetInterfaces().Where(i => typeof(IApplicationService).IsAssignableFrom(i)).ToArray();

                var hubServiceBuilder = serviceProvider.GetRequiredService<IHubServiceBuilder>();
                var hubServiceOptions = serviceProvider.GetRequiredService<IOptions<HubServiceOptions>>();

                foreach (var contract in contracts)
                {
                    hubServiceBuilder.AddContractHandler(contract, implementation);
                }

                var host = hubServiceBuilder.Build();
                context.Services.AddSingleton(host);
            }
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var hosts = context.ServiceProvider.GetServices<HubService>();
            foreach(var host in hosts)
            {
                host.Start();
            }
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            var hosts = context.ServiceProvider.GetServices<HubService>();
            foreach (var host in hosts)
            {
                host.Stop();
            }
        }
    }
}
