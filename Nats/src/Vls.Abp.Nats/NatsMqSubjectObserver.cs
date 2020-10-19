using NATS.Client;
using System;

namespace Vls.Abp.Nats
{
    public class NatsMqSubjectObserver : IObserver<Msg>
    {
        private readonly NatsConnectionPool _connectionManager;

        public NatsMqSubjectObserver(NatsConnectionPool connectionManager)
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
}
