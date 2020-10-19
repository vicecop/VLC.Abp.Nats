using Volo.Abp.Castle;
using Volo.Abp.Modularity;

namespace Vls.Abp.Examples.Client
{

    [DependsOn(
        typeof(AbpNatsMqModule), 
        typeof(AbpCastleCoreModule))]
    public class AbpNatsClientModule : AbpModule
    {

    }
}
