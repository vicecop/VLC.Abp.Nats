using System;

namespace Vls.Abp.EventStreamingBus
{
    public sealed class PipelineEventHandlerDisposeWrapper : IPipelineEventHandlerDisposeWrapper
    {
        public IPipelineEventHandler EventHandler { get; }

        private readonly Action _disposeAction;

        public PipelineEventHandlerDisposeWrapper(IPipelineEventHandler eventHandler, Action disposeAction = null)
        {
            _disposeAction = disposeAction;
            EventHandler = eventHandler;
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }
}
