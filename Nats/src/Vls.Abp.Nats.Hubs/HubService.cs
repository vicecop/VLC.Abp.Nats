using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vls.Abp.Nats.Hubs
{
    public class HubService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IEnumerable<HubContractHandler> _contractHandlers;

        private IConnection _connection;
        private IEnumerable<IAsyncSubscription> _subscriptions;

        public string ServiceUid { get; }
        public string ConnectionString { get; }

        public EventHandler<MsgHandlerEventArgs> MsgHandler { get; }

        public HubService(IServiceProvider serviceProvider, IEnumerable<HubContractHandler> contractHandlers,
            HubServiceOptions options, EventHandler<MsgHandlerEventArgs> eventHandler = null)
        {
            _serviceProvider = serviceProvider;
            _connectionFactory = _serviceProvider.GetRequiredService<ConnectionFactory>();
            _contractHandlers = contractHandlers ?? throw new ArgumentNullException(nameof(contractHandlers));

            ServiceUid = options?.ServiceUid ?? throw new ArgumentNullException(nameof(ServiceUid));
            ConnectionString = options?.ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));

            MsgHandler = eventHandler;
        }

        public void Start()
        {
            _connection = _connectionFactory.CreateConnection(ConnectionString);
            _subscriptions = _contractHandlers.SelectMany(handler => handler.Subscribe(_connection));

            foreach (var sub in _subscriptions)
                sub.Start();
        }

        public void Stop()
        {
            _connection.Drain();
        }
    }
}
