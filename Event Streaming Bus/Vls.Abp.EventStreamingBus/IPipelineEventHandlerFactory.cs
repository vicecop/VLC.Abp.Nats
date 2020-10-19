using System.Collections.Generic;

namespace Vls.Abp.EventStreamingBus
{
    public interface IPipelineEventHandlerFactory
    {
        IPipelineEventHandlerDisposeWrapper GetHandler();

        bool IsInFactories(List<IPipelineEventHandlerFactory> handlerFactories);
    }
}
