using Microsoft.Extensions.Options;
using NATS.Client;
using System;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Examples
{
    public sealed class NatsConnectionPool : INatsConnectionPool, ISingletonDependency, IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly AbpNatsMqOptions _options;

        private IConnection _connection;
        private bool _disposedValue;

        public NatsConnectionPool(IOptions<AbpNatsMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _connectionFactory = new ConnectionFactory();
            _options = options.Value;
        }

        public IConnection GetConnection
        {
            get
            {
                if (_connection == null)
                {
                    var options = ConnectionFactory.GetDefaultOptions();
                    options.Url = _options.Url;
                    options.Timeout = _options.ConnectionTimeout;

                    _connection = _connectionFactory.CreateConnection(options);
                }

                return _connection;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                    _connection = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
