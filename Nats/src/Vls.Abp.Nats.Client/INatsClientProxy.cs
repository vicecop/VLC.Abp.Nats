namespace Vls.Abp.Nats.Client
{

    public interface INatsClientProxy<out TRemoteService>
    {
        TRemoteService Service { get; }
    }
}
