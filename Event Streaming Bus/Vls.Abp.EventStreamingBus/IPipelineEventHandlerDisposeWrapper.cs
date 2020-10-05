using System;

namespace Vls.Abp.EventStreamingBus
{
    public interface IPipelineEventHandlerDisposeWrapper : IDisposable
    {
        IPipelineEventHandler EventHandler { get; }
    }
}
