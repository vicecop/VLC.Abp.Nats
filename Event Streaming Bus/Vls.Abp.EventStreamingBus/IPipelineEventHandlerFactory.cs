namespace Vls.Abp.EventStreamingBus
{
    public interface IPipelineEventHandlerFactory
    {
        IPipelineEventHandlerDisposeWrapper GetHandler();
    }
}
