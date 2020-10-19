using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace Vls.Abp.Examples.Store.Application
{
    public class StoreAppModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IDistributedEventHandler<TestEventEto>, TestEventHandler>();
        }
    }

    public interface IStoreApplicationService : IApplicationService
    {
        Task GetAsync();
    }

    public class StoreApplicationService : ApplicationService, IStoreApplicationService
    {
        public Task GetAsync()
        {
            return Task.CompletedTask;
        }
    }
}
