using NATS.Client;
using System;

namespace Vls.Abp.Nats
{
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
}
