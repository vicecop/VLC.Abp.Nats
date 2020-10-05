using System.Threading.Tasks;

namespace Vls.Abp.EventStreamingBus
{
    public interface IPipelineEventHandler
    {

    }

    public interface IPipelineEventHandler<in TEvent> : IPipelineEventHandler
    {
        Task HandleEventAsync(TEvent eventData);
    }
}
