using Volo.Abp.Modularity;

namespace Vls.Abp.Nats.Client
{
    [DependsOn(typeof(AbpNatsMqModule))]
    public class AbpNatsClientModule : AbpModule
    {

    }
}
