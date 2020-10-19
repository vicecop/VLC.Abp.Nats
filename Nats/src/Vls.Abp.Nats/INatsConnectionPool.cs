using NATS.Client;

namespace Vls.Abp.Examples
{
    public interface INatsConnectionPool
    {
        IConnection GetConnection { get; }
    }
}
