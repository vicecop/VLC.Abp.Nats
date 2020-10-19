using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using System;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Nats.Hubs
{
    public class HubServiceBuilder : IHubServiceBuilder, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly HubServiceOptions _serviceOptions;

        private List<HubContractHandler> _contractHandlers;
        private EventHandler<MsgHandlerEventArgs> _eventHandler;

        public HubServiceBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

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
            return AddContractHandler(typeof(TContract), typeof(TImplementation), factory);
        }

        public HubServiceBuilder AddContractHandler(Type contract, Type implementation, Func<IServiceProvider, ObjectFactory> factory = null)
        {
            if (!contract.IsAssignableFrom(implementation))
                throw new ArgumentException("Provided implementation does not inherit contract");

            var contractImplFactory = factory != null ? 
                factory.Invoke(_serviceProvider) :
                ActivatorUtilities.CreateFactory(implementation, Array.Empty<Type>());

            var handler = ActivatorUtilities.CreateInstance<HubContractHandler>(_serviceProvider, contract, _serviceOptions.ServiceUid, contractImplFactory);

            _contractHandlers.Add(handler);

            return this;
        }

        public HubService Build()
        {
            return ActivatorUtilities.CreateInstance<HubService>(_serviceProvider, _contractHandlers);
        }
    }
}
