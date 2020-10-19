using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace Vls.Abp.Nats.TestApplication
{
    public class TestApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IDistributedEventHandler<TestEventEto>, TestEventHandler>();
        }
    }
}
