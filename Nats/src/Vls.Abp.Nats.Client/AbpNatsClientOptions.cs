using System;
using System.Collections.Generic;

namespace Vls.Abp.Nats.Client
{
    public class AbpNatsClientOptions
    {
        public Dictionary<Type, NatsClientProxyConfig> NatsClientProxies { get; set; }

        public AbpNatsClientOptions()
        {
            NatsClientProxies = new Dictionary<Type, NatsClientProxyConfig>();
        }
    }
}
