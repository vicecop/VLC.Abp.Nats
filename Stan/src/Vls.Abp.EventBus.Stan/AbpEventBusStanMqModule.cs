using Microsoft.Extensions.DependencyInjection;
using Vls.Abp.Stan;
using Volo.Abp;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace Vls.Abp.EventBus.Stan
{
    [DependsOn(
        typeof(AbpEventBusModule),
        typeof(AbpStanMqModule))]
    public class AbpEventBusStanMqModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            context
                .ServiceProvider
                .GetRequiredService<StanMqDistributedEventBus>()
                .Initialize();
        }
    }
}
