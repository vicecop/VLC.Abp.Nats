using STAN.Client;

namespace Vls.Abp.Stan
{
    public interface IStanConnectionManager
    {
        IStanConnection Connection { get; }
    }
}
