using System;
using System.Threading.Tasks;

namespace Vls.Abp.EventStreamingBus
{
    public abstract partial class PipelineBusBase : IPipelineBus
    {
        private readonly IPipelineEventHandlerFactory _eventHandlerFactory;

        public PipelineBusBase(IPipelineEventHandlerFactory eventHandlerFactory)
        {
            _eventHandlerFactory = eventHandlerFactory;
        }

        public abstract Task PublishAsync<TKey, TEvent>(TKey key, TEvent eventDto) 
            where TKey : struct;

        public abstract void Subscribe<TKey, TEvent>(TKey key, IPipelineEventHandler<TEvent> handler)
            where TKey : struct;

        protected async Task TriggerHandlersAsync(Type eventType, object eventData)
        {
            await new SynchronizationContextRemover();

            using (var eventHandlerWrapper = _eventHandlerFactory.GetHandler())
            {
                var method = typeof(IPipelineEventHandler<>)
                .MakeGenericType(eventType)
                .GetMethod(
                    nameof(IPipelineEventHandler<object>.HandleEventAsync),
                    new[] { eventType }
                );

                await ((Task)method.Invoke(eventHandlerWrapper.EventHandler, new[] { eventData }));
            }
        }
    }
}
