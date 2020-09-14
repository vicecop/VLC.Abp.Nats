using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Collections.Generic;

namespace Vls.Abp.Nats.Hubs
{
    public class HubServiceBuilder : IHubServiceBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HubServiceFactory _serviceFactory;

        private readonly HubServiceOptions _serviceOptions;

        private List<HubContractHandler> _contractHandlers;
        private EventHandler<MsgHandlerEventArgs> _eventHandler;

        public HubServiceBuilder(IServiceProvider serviceProvider, HubServiceFactory serviceFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));

            _serviceOptions = HubServiceOptions.Default;
            _contractHandlers = new List<HubContractHandler>();
        }

        public HubServiceBuilder Configure(Action<HubServiceOptions> options)
        {
            options.Invoke(_serviceOptions);
            return this;
        }

        public HubServiceBuilder AddMsgEventHandler(EventHandler<MsgHandlerEventArgs> eventHandler)
        {
            _eventHandler = eventHandler;
            return this;
        }

        public HubServiceBuilder AddContractHandler<TContract, TImplementation>(Func<IServiceProvider, ObjectFactory> factory = null)
            where TContract : class
            where TImplementation : class, TContract
        {
            var serializer = _serviceProvider.GetRequiredService<INatsSerializer>();
            return AddContractHandler<TContract, TImplementation>(serializer, factory);
        }

        public HubServiceBuilder AddContractHandler<TContract, TImplementation>(INatsSerializer serializer, Func<IServiceProvider, ObjectFactory> factory)
            where TContract : class
            where TImplementation : class, TContract
        {
            var contractImplFactory = factory != null ? factory.Invoke(_serviceProvider) :
                ActivatorUtilities.CreateFactory(typeof(TImplementation), Array.Empty<Type>());

            var handlerLogger = _serviceProvider.GetService<ILogger<HubContractHandler>>();
            var handler = new HubContractHandler(handlerLogger, _serviceProvider, serializer, typeof(TContract), _serviceOptions.ServiceUid, contractImplFactory);
            _contractHandlers.Add(handler);

            return this;
        }

        public HubService Build()
        {
            return _serviceFactory.Create(_serviceOptions, _contractHandlers, _eventHandler);
        }
    }
}
