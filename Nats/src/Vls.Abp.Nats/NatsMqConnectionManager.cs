using Microsoft.Extensions.Options;
using NATS.Client;
using System;

namespace VLC.Abp.Nats
{
    public interface INatsMqConnectionManager
    {
        IConnection Connection { get; }
    }

    public class NatsMqSubjectObserver : IObserver<Msg>
    {
        private readonly NatsMqConnectionManager _connectionManager;

        public NatsMqSubjectObserver(NatsMqConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Msg value)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class NatsMqConnectionManager : INatsMqConnectionManager, IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _url;
        private readonly int _connectionTimeOut;

        public IConnection Connection { get; private set; }

        public NatsMqConnectionManager(IOptions<AbpNatsMqOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new System.ArgumentNullException(nameof(options));
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
