namespace Vls.Abp.Nats.Client
{
    public class NatsClientProxy<TRemoteService> : INatsClientProxy<TRemoteService>
    {
        public TRemoteService Service { get; }

        public NatsClientProxy(TRemoteService service)
        {
            Service = service;
        }
    }
}
