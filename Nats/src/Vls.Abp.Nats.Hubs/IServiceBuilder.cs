using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using System;

namespace Vls.Abp.Nats.Hubs
{
    public interface IServiceBuilder
    {
        ServiceBuilder Configure(Action<ServiceOptions> options);
        ServiceBuilder AddMsgEventHandler(EventHandler<MsgHandlerEventArgs> eventHandler);
        ServiceBuilder AddContractHandler<TContract, TImplementation>(Func<IServiceProvider, ObjectFactory> factory = null)
            where TContract : class
            where TImplementation : class, TContract;
        ServiceBuilder AddContractHandler<TContract, TImplementation>(INatsSerializer serializer, Func<IServiceProvider, ObjectFactory> factory = null)
            where TContract : class
            where TImplementation : class, TContract;
    }
}
