using NATS.Client;

namespace Vls.Abp.Nats
{
    public interface INatsMqConnectionManager
    {
        IConnection Connection { get; }
    }
}
