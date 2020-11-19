using Microsoft.Extensions.Options;
using STAN.Client;
using System;
using System.Collections.Concurrent;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Stan
{
    public sealed class StanConnectionPool : IStanConnectionPool, ISingletonDependency
    {
        private readonly AbpStanMqOptions _options;
        private readonly ConcurrentDictionary<string, IStanConnection> _connections;
        private readonly StanConnectionFactory _connectionFactory;

        private bool _disposedValue;

        public StanConnectionPool(IOptions<AbpStanMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _connections = new ConcurrentDictionary<string, IStanConnection>();
            _connectionFactory = new StanConnectionFactory();
            _options = options.Value;
        }

        public IStanConnection GetConnection(string name)
        {
            return _connections.GetOrAdd(name, newName =>
            {
                var options = StanOptions.GetDefaultOptions();
                options.NatsURL = _options.Url;
                options.ConnectTimeout = _options.ConnectionTimeout;

                return _connectionFactory.CreateConnection(_options.ClusterId, _options.ClientId, options);
            });
        }


        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var connection in _connections.Values)
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
