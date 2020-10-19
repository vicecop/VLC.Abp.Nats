using Microsoft.Extensions.Options;
using STAN.Client;
using System;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Stan
{
    public sealed class StanConnectionPool : IStanConnectionPool, ISingletonDependency
    {
        private StanConnectionFactory _connectionFactory;
        private AbpStanMqOptions _options;

        private IStanConnection _connection;
        private bool _disposedValue;

        public IStanConnection GetConnection 
        { 
            get
            {
                if (_connection == null)
                {
                    var options = StanOptions.GetDefaultOptions();
                    options.NatsURL = _options.Url;
                    options.ConnectTimeout = _options.ConnectionTimeout;

                    _connection = _connectionFactory.CreateConnection(_options.ClusterId, _options.ClientId, options);
                }

                return _connection;
            }
        }

        public StanConnectionPool(IOptions<AbpStanMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _connectionFactory = new StanConnectionFactory();
            _options = options.Value;
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
