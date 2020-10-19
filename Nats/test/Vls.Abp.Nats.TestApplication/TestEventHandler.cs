using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Vls.Abp.Nats.TestApplication
{
    public class TestEventHandler : IDistributedEventHandler<TestEventEto>, ITransientDependency
    {
        public TestEventHandler()
        {

        }

        public Task HandleEventAsync(TestEventEto eventData)
        {
            return Task.CompletedTask;
        }
    }
}
