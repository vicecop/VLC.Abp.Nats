using Volo.Abp.Modularity;

namespace Vls.Abp.Nats.Hubs
{
    [DependsOn(typeof(AbpNatsMqModule))]
    public class AbpNatsHubsModule : AbpModule
    {

    }
}
