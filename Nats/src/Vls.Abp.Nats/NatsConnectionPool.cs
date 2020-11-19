using Microsoft.Extensions.Options;
using NATS.Client;
using System;
using System.Collections.Concurrent;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Nats
{
    public sealed class NatsConnectionPool : INatsConnectionPool, ISingletonDependency, IDisposable
    {
        private readonly ConcurrentDictionary<string, IConnection> _connections;
        private readonly ConnectionFactory _connectionFactory;
        private readonly AbpNatsMqOptions _options;

        private bool _disposedValue;

        public NatsConnectionPool(IOptions<AbpNatsMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _connections = new ConcurrentDictionary<string, IConnection>();
            _connectionFactory = new ConnectionFactory();
            _options = options.Value;
        }

        public IConnection GetConnection(string name)
        {
            return _connections.GetOrAdd(name, newName =>
            {
                var options = ConnectionFactory.GetDefaultOptions();
                options.Url = _options.Url;
                options.Timeout = _options.ConnectionTimeout;

                return _connectionFactory.CreateConnection(options);
            });
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach(var connection in _connections.Values)
                    {
                        connection.Dispose();
                    }

                    _connections.Clear();
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
