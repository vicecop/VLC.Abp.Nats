using Microsoft.Extensions.Options;
using STAN.Client;
using System;
using Volo.Abp.DependencyInjection;

namespace Vls.Abp.Stan
{
    public sealed class StanConnectionManager : IStanConnectionManager, ISingletonDependency, IDisposable
    {
        private StanConnectionFactory _connectionFactory;
        private AbpStanMqOptions _options;

        public IStanConnection Connection { get; private set; }

        public StanConnectionManager(IOptions<AbpStanMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _connectionFactory = new StanConnectionFactory();
            _options = options.Value;
        }

        public void Connect()
        {
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = _options.Url;
            options.ConnectTimeout = _options.ConnectionTimeout;

            Connection = _connectionFactory.CreateConnection(_options.ClusterId, _options.ClientId, options);
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
