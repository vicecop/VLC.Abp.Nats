using STAN.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vls.Abp.Stan;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.EventStreamingBus
{
    public class StanPipelineBus : PipelineBusBase, ISingletonDependency
    {
        private readonly IStanConnectionManager _connectionManager;
        private readonly IStanSerializer _serializer;

        protected ConcurrentDictionary<string, Type> EventTypes { get; }

        public StanPipelineBus(IStanConnectionManager connectionManager, IStanSerializer serializer)
        {
            _connectionManager = connectionManager;
            _serializer = serializer;
        }

        public override Task PublishAsync<TKey, TEvent>(TKey key, TEvent eventDto)
        {
            throw new NotImplementedException();
        }

        public override void Subscribe<TKey, TEvent>(TKey key, IPipelineEventHandler<TEvent> handler)
        {
            var subscription = _connectionManager.Connection.Subscribe(key.ToString(), ProcessStanEvent);
        }

        private void ProcessStanEvent(object sender, StanMsgHandlerArgs args)
        {
            var eventName = args.Message.Subject;
            var eventType = EventTypes.GetOrDefault(eventName);
            if (eventType == null)
            {
                return;
            }

            var eventData = _serializer.Deserialize(args.Message.Data, eventType);


            _serializer.Deserialize
        }
    }
}
