using Microsoft.Extensions.DependencyInjection;
using System;

namespace Vls.Abp.EventStreamingBus
{
    public class IocPipelineEventHandlerFactory : IPipelineEventHandlerFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Type _handlerType;

        public IocPipelineEventHandlerFactory(IServiceScopeFactory scopeFactory, Type handlerType)
        {
            _scopeFactory = scopeFactory;
            _handlerType = handlerType;
        }

        public IPipelineEventHandlerDisposeWrapper GetHandler()
        {
            var scope = _scopeFactory.CreateScope();
            return new PipelineEventHandlerDisposeWrapper(
                (IPipelineEventHandler)scope.ServiceProvider.GetRequiredService(_handlerType), 
                () => scope.Dispose());
        }
    }
}
