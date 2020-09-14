using NATS.Client;
using System;
using System.Collections.Generic;

namespace Vls.Abp.Nats.Hubs
{
    public class HubServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public HubServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public HubService Create(HubServiceOptions options, IEnumerable<HubContractHandler> contractHandlers,
            EventHandler<MsgHandlerEventArgs> msgHandler = null)
        {
            return new HubService(_serviceProvider, contractHandlers, options, msgHandler);
        }
    }
}
