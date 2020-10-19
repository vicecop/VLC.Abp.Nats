using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using STAN.Client;
using STAN.Client.Rx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vls.Abp.Stan;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

namespace Vls.Abp.EventStreamingBus
{
    public sealed class StanPipelineBusSubscriptionManager : IDisposable
    {
        private ConcurrentDictionary<string, IStanSubscription> _subscriptions;
        private readonly IStanConnectionPool _connectionPool;

        public StanPipelineBusSubscriptionManager(IStanConnectionPool connectionPool)
        {
            _subscriptions = new ConcurrentDictionary<string, IStanSubscription>();
            _connectionPool = connectionPool;
        }

        public void Subscribe(object key, EventHandler<StanMsgHandlerArgs> eventHandler) 
        {
            _subscriptions.GetOrAdd(
                key.ToString(),
                subKey => _connectionPool.GetConnection.Subscribe(subKey, eventHandler));
        }

        public void Unsubscribe(object key)
        {
            if (_subscriptions.TryGetValue(key.ToString(), out var subscription))
            {
                subscription.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var sub in _subscriptions.Values)
                sub.Dispose();

            _subscriptions.Clear();
        }
    }

    public class PipelineEventHandler
    {
        public PipelineContext Context { get; internal set; }
    }

    public class PipelineEventHandler<TEvent> : PipelineEventHandler, IPipelineEventHandler<TEvent>
    {
        public Task HandleEventAsync(TEvent eventData)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DatagramAttribute : Attribute
    {
        public string Version { get; set; }
    }

    public sealed class HeaderAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public sealed class DataAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [Datagram]
    public class TestDatagram
    {
        [Header]
        public string Name { get; set; }
        [Data]
        public string EPC { get; set; }
        [Data]
        public double Weight { get; set; }
    }

    public sealed class PipelineDatagram
    {
        public IDictionary<string, string> Headers { get; private set; }
        public IDictionary<string, string> Data { get; private set; }
        public long Timestamp { get; private set; }

        public string Version => Headers[PipelineDatagramConsts.Version];

        public PipelineDatagram()
        {
            Headers = new Dictionary<string, string>();
            Data = new Dictionary<string, string>();
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public PipelineDatagram(IDictionary<string, string> headers, IDictionary<string, string> data)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static PipelineDatagram Combine(IEnumerable<PipelineDatagram> datagrams)
        {
            return datagrams.Aggregate(new PipelineDatagram(), (seed, next) => seed.Combine(next));
        }

        public static PipelineDatagram Combine(params PipelineDatagram[] datagrams)
        {
            return Combine((IEnumerable<PipelineDatagram>)datagrams);
        }
    }

    public static class PipelineDatagramConsts
    {
        public const string Version = "Version";
        public const string Type = "Type";
        public const string Tag = "Tag";
        public const string EPC = "EPC";
        public const string Weight = "Weight";
    }

    public static class PipelineDatagramExtensions
    {
        public static PipelineDatagram Combine(this PipelineDatagram original, PipelineDatagram target)
        {
            original.Headers.AddIfNotContains(target.Headers);
            original.Data.AddIfNotContains(target.Data);
            return original;
        }

        public static Type GetTypeFromHeaders(this PipelineDatagram datagram) =>
            Type.GetType(datagram.Headers[PipelineDatagramConsts.Type]);

        public static bool HasTag(this PipelineDatagram datagram) =>
            datagram.Data.Any(i => i.Key == PipelineDatagramConsts.Tag);

        public static bool HasWeight(this PipelineDatagram datagram) =>
            datagram.Data.Any(i => i.Key == PipelineDatagramConsts.Weight);
    }

    public class PipelineContext
    {
        public PipelineDatagram Datagram { get; }
        public DateTimeOffset OccuredAt => DateTimeOffset.FromUnixTimeMilliseconds(Datagram.Timestamp);
        public DateTimeOffset ReceivedAt { get; }
    }

    public static class PipelineEventContextExtensions
    {

    }

    public interface IPipelineDatagramSerializer
    {
        PipelineDatagram Serialize(object obj);
        object Deserialize(Type type, PipelineDatagram datagram);
        object Deserialize(PipelineDatagram datagram);
        T Deserialize<T>(PipelineDatagram datagram);
    }

    public class PipelineDatagramJsonSerializer : IPipelineDatagramSerializer
    {
        private readonly IJsonSerializer _jsonSerializer;

        public PipelineDatagramJsonSerializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public object Deserialize(Type type, PipelineDatagram datagram)
        {
            var headersProps = type.GetProperties().Where(p => p.IsDefined(typeof(HeaderAttribute), true));
            var dataProps = type.GetProperties().Where(p => p.IsDefined(typeof(DataAttribute), true));

            var obj = Activator.CreateInstance(type);
            foreach (var prop in headersProps)
            {
                prop.SetValue(obj, _jsonSerializer.Deserialize(prop.PropertyType, datagram.Headers[prop.Name]));
            }

            foreach (var prop in dataProps)
            {
                prop.SetValue(obj, _jsonSerializer.Deserialize(prop.PropertyType, datagram.Data[prop.Name]));
            }

            return obj;
        }

        public object Deserialize(PipelineDatagram datagram)
        {
            return Deserialize(datagram.GetTypeFromHeaders(), datagram);
        }

        public T Deserialize<T>(PipelineDatagram datagram)
        {
            return (T)Deserialize(typeof(T), datagram);
        }

        public PipelineDatagram Serialize(object obj)
        {
            var type = obj.GetType();

            var headersProps = type.GetProperties().Where(p => p.IsDefined(typeof(HeaderAttribute), true));
            var dataProps = type.GetProperties().Where(p => p.IsDefined(typeof(DataAttribute), true));

            var headers = headersProps.ToDictionary(prop => $"{type.Name}_{prop.Name}", prop => _jsonSerializer.Serialize(prop.GetValue(obj)));
            headers.Add(PipelineDatagramConsts.Type, type.FullName);
            var data = dataProps.ToDictionary(prop => $"{type.Name}_{prop.Name}", prop => _jsonSerializer.Serialize(prop.GetValue(obj)));

            var datagram = new PipelineDatagram(headers, data);

            return datagram;
        }
    }

    public class StanPipelineBus : PipelineBusBase, ISingletonDependency
    {
        private readonly IStanConnectionPool _connectionPool;
        private readonly IStanSerializer _stanSerializer;
        private readonly IPipelineDatagramSerializer _datagramSerializer;

        protected ConcurrentDictionary<string, Type> EventTypes { get; }

        private ConcurrentDictionary<string, IStanSubscription> _subscriptions;
        private ConcurrentDictionary<string, INATSObservable<PipelineDatagram>> _channels;
        protected ConcurrentDictionary<Type, List<IPipelineEventHandlerFactory>> HandlerFactories { get; }

        public StanPipelineBus(IServiceScopeFactory scopeFactory, 
            IStanConnectionPool connectionPool, IStanSerializer serializer)
            : base(scopeFactory)
        {
            _connectionPool = connectionPool;
            _stanSerializer = serializer;
        }

        public override void RegisterEventHandlerFactory<TEvent>(IPipelineEventHandlerFactory factory)
        {
            RegisterEventHandlerFactory(typeof(TEvent), factory);
        }

        public override void RegisterEventHandlerFactory(Type eventType, IPipelineEventHandlerFactory factory)
        {
            var handlerFactories = GetOrCreateHandlerFactories(eventType);

            if (factory.IsInFactories(handlerFactories))
            {
                return;
            }

            handlerFactories.Add(factory);
        }

        public override async Task PublishAsync<TKey, TEvent>(TKey key, TEvent eventDto)
        {
            var eventName = key.ToString();
            var datagram = _datagramSerializer.Serialize(eventDto);
            var body = _stanSerializer.Serialize(datagram);

            await _connectionPool.GetConnection.PublishAsync(eventName, body);
        }

        public override void Subscribe<TKey>(TKey key)
        {
            _subscriptions.GetOrAdd(
                key.ToString(),
                subKey => _connectionPool.GetConnection.Subscribe(subKey, ProcessStanEvent));
        }

        public override void Unsubscribe<TKey>(TKey key)
        {
            if (_subscriptions.TryGetValue(key.ToString(), out var subscription))
            {
                subscription.Dispose();
            }  
        }

        private List<IPipelineEventHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
        {
            return HandlerFactories.GetOrAdd(
                eventType,
                type =>
                {
                    var eventName = type.Name;
                    EventTypes[eventName] = type;
                    return new List<IPipelineEventHandlerFactory>();
                }
            );
        }

        private async void ProcessStanEvent(object sender, StanMsgHandlerArgs args)
        {
            var eventName = args.Message.Subject;
            var eventType = EventTypes.GetOrDefault(eventName);
            if (eventType == null)
            {
                return;
            }

            var datagram = _stanSerializer.Deserialize<PipelineDatagram>(args.Message.Data);
            var eventData = _datagramSerializer.Deserialize(datagram);

            await TriggerHandlersAsync(eventType, eventData);
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

        public override IObservable<PipelineDatagram> GetChannel(object key)
        {
            return _channels.GetOrAdd(key.ToString(), strKey => 
                _connectionPool.GetConnection
                    .Observe(strKey)
                    .Select(msg => _stanSerializer.Deserialize<PipelineDatagram>(msg.Data)));
        }
    }
}
