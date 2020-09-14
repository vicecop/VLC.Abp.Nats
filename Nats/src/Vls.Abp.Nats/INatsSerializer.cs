using System;

namespace Vls.Abp.Nats
{
    public interface INatsSerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] value, Type type);
        T Deserialize<T>(byte[] value);
    }
}
