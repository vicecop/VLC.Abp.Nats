using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using STAN.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vls.Abp.Stan;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace Vls.Abp.EventBus.Nats
{
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IDistributedEventBus), typeof(StanMqDistributedEventBus))]
    public class StanMqDistributedEventBus : EventBusBase, IDistributedEventBus, ISingletonDependency
    {
        private readonly IStanConnectionPool _connectionManager;

        protected ConcurrentDictionary<Type, List<IEventHandlerFactory>> HandlerFactories { get; }
        protected ConcurrentDictionary<string, Type> EventTypes { get; }

        protected IStanSerializer Serializer { get; }
        protected AbpDistributedEventBusOptions DistributedEventBusOptions { get; }

        public StanMqDistributedEventBus(
            IStanConnectionPool connectionManager,
            IServiceScopeFactory serviceScopeFactory,
            ICurrentTenant currentTenant,
            IStanSerializer serializer,
            IOptions<AbpDistributedEventBusOptions> distributedEventBusOptions)
            : base(serviceScopeFactory, currentTenant)
        {
            _connectionManager = connectionManager;

            Serializer = serializer;
            DistributedEventBusOptions = distributedEventBusOptions.Value;

            HandlerFactories = new ConcurrentDictionary<Type, List<IEventHandlerFactory>>();
            EventTypes = new ConcurrentDictionary<string, Type>();
        }

        public void Initialize()
        {
            SubscribeHandlers(DistributedEventBusOptions.Handlers);

            foreach (var eventName in EventTypes.Keys)
            {
                var subscription = _connectionManager.GetConnection.Subscribe(eventName, ProcessEventAsync);
            }
        }

        private async void ProcessEventAsync(object sender, StanMsgHandlerArgs e)
        {
            var eventName = e.Message.Subject;
            var eventType = EventTypes.GetOrDefault(eventName);
            if (eventType == null)
            {
                return;
            }

            var eventData = Serializer.Deserialize(e.Message.Data, eventType);

            await TriggerHandlersAsync(eventType, eventData);
        }

        public override Task PublishAsync(Type eventType, object eventData)
        {
            var eventName = EventNameAttribute.GetNameOrDefault(eventType);
            var body = Serializer.Serialize(eventData);

            _connectionManager.GetConnection.Publish(eventName, body);

            return Task.CompletedTask;
        }

        public IDisposable Subscribe<TEvent>(IDistributedEventHandler<TEvent> handler) 
            where TEvent : class
        {
            return Subscribe(typeof(TEvent), handler);
        }

        public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
        {
            var handlerFactories = GetOrCreateHandlerFactories(eventType);

            if (factory.IsInFactories(handlerFactories))
            {
                return NullDisposable.Instance;
            }

            handlerFactories.Add(factory);

            return new EventHandlerFactoryUnregistrar(this, eventType, factory);
        }

        public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
        {
            Check.NotNull(action, nameof(action));
            GetOrCreateHandlerFactories(typeof(TEvent))
                .Locking(factories =>
                {
                    factories.RemoveAll(
                        factory =>
                        {
                            var singleInstanceFactory = factory as SingleInstanceHandlerFactory;
                            if (singleInstanceFactory == null)
                            {
                                return false;
                            }

                            var actionHandler = singleInstanceFactory.HandlerInstance as ActionEventHandler<TEvent>;
                            if (actionHandler == null)
                            {
                                return false;
                            }

                            return actionHandler.Action == action;
                        });
                });
        }

        public override void Unsubscribe(Type eventType, IEventHandler handler)
        {
            GetOrCreateHandlerFactories(eventType)
                .Locking(factories =>
                {
                    factories.RemoveAll(
                        factory =>
                            factory is SingleInstanceHandlerFactory &&
                            (factory as SingleInstanceHandlerFactory).HandlerInstance == handler
                    );
                });
        }

        public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(eventType).Locking(factories => factories.Remove(factory));
        }

        public override void UnsubscribeAll(Type eventType)
        {
            GetOrCreateHandlerFactories(eventType).Locking(factories => factories.Clear());
        }

        private List<IEventHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
        {
            return HandlerFactories.GetOrAdd(
                eventType,
                type =>
                {
                    var eventName = EventNameAttribute.GetNameOrDefault(type);
                    EventTypes[eventName] = type;
                    return new List<IEventHandlerFactory>();
                }
            );
        }

        protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
        {
            var handlerFactoryList = new List<EventTypeWithEventHandlerFactories>();

            foreach (var handlerFactory in HandlerFactories.Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key)))
            {
                handlerFactoryList.Add(new EventTypeWithEventHandlerFactories(handlerFactory.Key, handlerFactory.Value));
            }

            return handlerFactoryList.ToArray();
        }

        private static bool ShouldTriggerEventForHandler(Type targetEventType, Type handlerEventType)
        {
            //Should trigger same type
            if (handlerEventType == targetEventType)
            {
                return true;
            }

            //TODO: Support inheritance? But it does not support on subscription to RabbitMq!
            //Should trigger for inherited types
            if (handlerEventType.IsAssignableFrom(targetEventType))
            {
                return true;
            }

            return false;
        }
    }
}
