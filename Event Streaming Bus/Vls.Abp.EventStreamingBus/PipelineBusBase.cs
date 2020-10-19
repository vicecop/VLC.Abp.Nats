using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.Collections;

namespace Vls.Abp.EventStreamingBus
{
    public abstract partial class PipelineBusBase : IPipelineBus
    {
        protected IServiceScopeFactory ServiceScopeFactory { get; }

        public PipelineBusBase(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public abstract Task PublishAsync<TKey, TEvent>(TKey key, TEvent eventDto) 
            where TKey : struct
            where TEvent : class;

        public abstract void RegisterEventHandlerFactory<TEvent>(IPipelineEventHandlerFactory factory)
            where TEvent : class;

        public abstract void RegisterEventHandlerFactory(Type eventType, IPipelineEventHandlerFactory factory);

        protected virtual void RegisterHandlers(ITypeList<IPipelineEventHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!typeof(IPipelineEventHandler).GetTypeInfo().IsAssignableFrom(@interface))
                    {
                        continue;
                    }

                    var genericArgs = @interface.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        RegisterEventHandlerFactory(genericArgs[0], new IocPipelineEventHandlerFactory(ServiceScopeFactory, handler));
                    }
                }
            }
        }

        public abstract void Subscribe<TKey>(TKey key)
            where TKey : struct;

        public abstract void Unsubscribe<TKey>(TKey key)
            where TKey : struct;

        protected abstract IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType);

        public virtual async Task TriggerHandlersAsync(Type eventType, object eventData)
        {
            var exceptions = new List<Exception>();

            await TriggerHandlersAsync(eventType, eventData, exceptions);

            if (exceptions.Any())
            {
                if (exceptions.Count == 1)
                {
                    exceptions[0].ReThrow();
                }

                throw new AggregateException("More than one error has occurred while triggering the event: " + eventType, exceptions);
            }
        }

        protected virtual async Task TriggerHandlersAsync(Type eventType, object eventData, List<Exception> exceptions)
        {
            await new SynchronizationContextRemover();

            foreach (var handlerFactories in GetHandlerFactories(eventType))
            {
                foreach (var handlerFactory in handlerFactories.EventHandlerFactories)
                {
                    await TriggerHandlerAsync(handlerFactory, handlerFactories.EventType, eventData, exceptions);
                }
            }
        }

        protected async Task TriggerHandlerAsync(IPipelineEventHandlerFactory eventHandlerFactory, Type eventType, object eventData, List<Exception> exceptions)
        {
            using (var eventHandlerWrapper = eventHandlerFactory.GetHandler())
            {
                try
                {
                    var method = typeof(IPipelineEventHandler<>)
                        .MakeGenericType(eventType)
                        .GetMethod(
                            nameof(IPipelineEventHandler<object>.HandleEventAsync),
                            new[] { eventType });

                    await ((Task)method.Invoke(eventHandlerWrapper.EventHandler, new[] { eventData }));
                }
                catch (TargetInvocationException ex)
                {
                    exceptions.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
        }

        public abstract IObservable<PipelineDatagram> GetChannel(object key);

        protected class EventTypeWithEventHandlerFactories
        {
            public Type EventType { get; }

            public List<IPipelineEventHandlerFactory> EventHandlerFactories { get; }

            public EventTypeWithEventHandlerFactories(Type eventType, List<IPipelineEventHandlerFactory> eventHandlerFactories)
            {
                EventType = eventType;
                EventHandlerFactories = eventHandlerFactories;
            }
        }
    }
}
