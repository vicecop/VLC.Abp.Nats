using Microsoft.Extensions.Options;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vls.Abp.Examples.Hubs
{
    public class HubService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INatsConnectionPool _connectionPool;
        private readonly IEnumerable<HubContractHandler> _contractHandlers;

        private IEnumerable<IAsyncSubscription> _subscriptions;

        public string ServiceUid { get; }
        public string ConnectionString { get; }

        public EventHandler<MsgHandlerEventArgs> MsgHandler { get; }

        public HubService(IOptions<HubServiceOptions> options, IServiceProvider serviceProvider, INatsConnectionPool connectionPool, List<HubContractHandler> contractHandlers/*, EventHandler<MsgHandlerEventArgs> eventHandler = null*/)
        {
            _serviceProvider = serviceProvider;
            _connectionPool = connectionPool;
            _contractHandlers = contractHandlers ?? throw new ArgumentNullException(nameof(contractHandlers));

            ServiceUid = options?.Value?.ServiceUid ?? throw new ArgumentNullException(nameof(ServiceUid));

            //MsgHandler = eventHandler;
        }

        public void Start()
        {
            var connection = _connectionPool.GetConnection;
            _subscriptions = _contractHandlers.SelectMany(handler => handler.Subscribe(connection));

            foreach (var sub in _subscriptions)
                sub.Start();
        }

        public void Stop()
        {
            //_connection.Drain();
        }
    }
}
