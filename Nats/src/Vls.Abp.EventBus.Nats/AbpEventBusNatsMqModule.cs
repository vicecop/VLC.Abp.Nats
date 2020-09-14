using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using VLC.Abp.Nats;
using Volo.Abp;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace Vlc.Abp.EventBus.Nats
{
    [DependsOn(
        typeof(AbpEventBusModule),
        typeof(AbpNatsMqModule))]
    public class AbpEventBusNatsMqModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);
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
