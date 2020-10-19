using Vls.Abp.Nats;
using Volo.Abp.Castle;
using Volo.Abp.Modularity;

namespace Vls.Abp.Nats.Client
{
    [DependsOn(
        typeof(AbpNatsMqModule), 
        typeof(AbpCastleCoreModule))]
    public class AbpNatsClientModule : AbpModule
    {

    }
}
