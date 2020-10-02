using Microsoft.Extensions.Options;
using NATS.Client;
using System;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Nats
{
    public sealed class NatsMqConnectionManager : INatsMqConnectionManager, ISingletonDependency, IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _url;
        private readonly int _connectionTimeOut;

        public IConnection Connection { get; private set; }

        public NatsMqConnectionManager(IOptions<AbpNatsMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _connectionFactory = new ConnectionFactory();
            _url = options.Value.Url;
            _connectionTimeOut = options.Value.ConnectionTimeout;
        }

        public void Connect()
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = _url;
            options.Timeout = _connectionTimeOut;
            Connection = _connectionFactory.CreateConnection(options);
        }

        public void Disconnect()
        {
            Connection?.Close();
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
