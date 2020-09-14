using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Json;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace Vls.Abp.Nats
{
    [DependsOn(
        typeof(AbpJsonModule),
        typeof(AbpThreadingModule)
        )]
    public class AbpNatsMqModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<AbpNatsMqOptions>(configuration.GetSection("Nats"));

            context.Services.AddSingleton<NatsMqConnectionManager>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var connManager = context.ServiceProvider.GetRequiredService<NatsMqConnectionManager>();
            connManager.Connect();
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            var connManager = context.ServiceProvider.GetRequiredService<NatsMqConnectionManager>();
            connManager.Disconnect();
            connManager.Dispose();
        }
    }
}
