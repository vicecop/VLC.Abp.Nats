using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vls.Abp.EventStreamingBus
{
    public class IocPipelineEventHandlerFactory : IPipelineEventHandlerFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public Type HandlerType { get; }

        public IocPipelineEventHandlerFactory(IServiceScopeFactory scopeFactory, Type handlerType)
        {
            _scopeFactory = scopeFactory;
            HandlerType = handlerType;
        }

        public IPipelineEventHandlerDisposeWrapper GetHandler()
        {
            var scope = _scopeFactory.CreateScope();
            return new PipelineEventHandlerDisposeWrapper(
                (IPipelineEventHandler)scope.ServiceProvider.GetRequiredService(HandlerType), 
                () => scope.Dispose());
        }

        public bool IsInFactories(List<IPipelineEventHandlerFactory> handlerFactories)
        {
            return handlerFactories
                .OfType<IocPipelineEventHandlerFactory>()
                .Any(f => f.HandlerType == HandlerType);
        }
    }
}
