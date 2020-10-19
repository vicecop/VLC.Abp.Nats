using System;
using System.Threading.Tasks;

namespace Vls.Abp.EventStreamingBus
{
    public interface IPipelineBus
    {
        Task PublishAsync<TKey, TEvent>(TKey key, TEvent @eventDto)
            where TKey : struct
            where TEvent : class;

        IObservable<PipelineDatagram> GetChannel(object key);

        void Subscribe<TKey>(TKey key)
            where TKey : struct;

        void Unsubscribe<TKey>(TKey key)
            where TKey : struct;
    }
}
