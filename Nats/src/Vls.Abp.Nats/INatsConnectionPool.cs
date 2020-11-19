using NATS.Client;

namespace Vls.Abp.Nats
{
    public interface INatsConnectionPool
    {
        IConnection GetConnection(string name);
    }
}
