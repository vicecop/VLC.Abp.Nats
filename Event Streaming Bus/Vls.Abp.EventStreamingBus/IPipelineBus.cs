using System.Threading.Tasks;

namespace Vls.Abp.EventStreamingBus
{
    public interface IPipelineBus
    {
        Task PublishAsync<TKey, TEvent>(TKey key, TEvent @eventDto)
            where TKey : struct;

        void Subscribe<TKey, TEvent>(TKey key, IPipelineEventHandler<TEvent> handler)
            where TKey : struct;
    }
}
