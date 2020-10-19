using Microsoft.Extensions.DependencyInjection;
using Vls.Abp.Examples;
using Volo.Abp;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace Vls.Abp.EventBus.Nats
{
    [DependsOn(
        typeof(AbpEventBusModule),
        typeof(AbpNatsMqModule))]
    public class AbpEventBusNatsMqModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            context
                .ServiceProvider
                .GetRequiredService<NatsMqDistributedEventBus>()
                .Initialize();
        }
    }
}
