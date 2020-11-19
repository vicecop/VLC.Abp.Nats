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
    public sealed class StanMqDistributedEventBus : EventBusBase, IDistributedEventBus, IDisposable, ISingletonDependency
    {
        private readonly IStanConnectionPool _connectionManager;

        private readonly ConcurrentDictionary<Type, List<IEventHandlerFactory>> _handlerFactories;
        private readonly ConcurrentDictionary<string, Type> _eventTypes;
        private readonly ConcurrentDictionary<string, IDisposable> _eventSubscriptions;

        private readonly IStanSerializer _serializer;
        private readonly AbpDistributedEventBusOptions _distributedEventBusOptions;

        public StanMqDistributedEventBus(
            IStanConnectionPool connectionManager,
            IServiceScopeFactory serviceScopeFactory,
            ICurrentTenant currentTenant,
            IStanSerializer serializer,
            IOptions<AbpDistributedEventBusOptions> distributedEventBusOptions)
            : base(serviceScopeFactory, currentTenant)
        {
            _connectionManager = connectionManager;

            _serializer = serializer;
            _distributedEventBusOptions = distributedEventBusOptions.Value;

            _handlerFactories = new ConcurrentDictionary<Type, List<IEventHandlerFactory>>();
            _eventTypes = new ConcurrentDictionary<string, Type>();
            _eventSubscriptions = new ConcurrentDictionary<string, IDisposable>();
        }

        public void Initialize()
        {
            SubscribeHandlers(_distributedEventBusOptions.Handlers);

            foreach (var eventName in _eventTypes.Keys)
            {
                var subscription = _connectionManager.GetConnection(nameof(StanMqDistributedEventBus))
                    .Subscribe(eventName, ProcessEventAsync);
                _eventSubscriptions.TryAdd(eventName, subscription);
            }
        }

        private async void ProcessEventAsync(object sender, StanMsgHandlerArgs e)
        {
            var eventName = e.Message.Subject;
            var eventType = _eventTypes.GetOrDefault(eventName);
            if (eventType == null)
            {
                return;
            }

            var eventData = _serializer.Deserialize(e.Message.Data, eventType);

            await TriggerHandlersAsync(eventType, eventData);
        }

        public override Task PublishAsync(Type eventType, object eventData)
        {
            var eventName = EventNameAttribute.GetNameOrDefault(eventType);
            var body = _serializer.Serialize(eventData);

            _connectionManager.GetConnection(nameof(StanMqDistributedEventBus)).Publish(eventName, body);

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
            return _handlerFactories.GetOrAdd(
                eventType,
                type =>
                {
                    var eventName = EventNameAttribute.GetNameOrDefault(type);
                    _eventTypes[eventName] = type;
                    return new List<IEventHandlerFactory>();
                }
            );
        }

        protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
        {
            var handlerFactoryList = new List<EventTypeWithEventHandlerFactories>();

            foreach (var handlerFactory in _handlerFactories.Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key)))
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

        public void Dispose()
        {
            foreach (var subscription in _eventSubscriptions.Values)
            {
                subscription.Dispose();
            }

            _eventSubscriptions.Clear();
        }
    }
}
