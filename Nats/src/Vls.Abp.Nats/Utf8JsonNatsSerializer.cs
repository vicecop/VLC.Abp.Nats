using System;
using System.Text;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

namespace Vls.Abp.Nats
{
    public class Utf8JsonNatsSerializer : INatsSerializer, ITransientDependency
    {
        private readonly IJsonSerializer _jsonSerializer;

        public Utf8JsonNatsSerializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(obj));
        }

        public object Deserialize(byte[] value, Type type)
        {
            return _jsonSerializer.Deserialize(type, Encoding.UTF8.GetString(value));
        }

        public byte[] Serialize(object[] objs)
        {
            return Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(objs));
        }

        public object[] Deserialize(byte[] value, Type[] types)
        {
            return _jsonSerializer.Deserialize()
        }
    }
}
