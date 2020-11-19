using STAN.Client;
using System;

namespace Vls.Abp.Stan
{
    public interface IStanConnectionPool : IDisposable
    {
        IStanConnection GetConnection(string name);
    }
}
