using System;

namespace Vls.Abp.Nats
{
    public interface INatsSerializer
    {
        byte[] Serialize(object obj);
        byte[] Serialize(object[] objs);
        object Deserialize(byte[] value, Type type);
        object[] Deserialize(byte[] value, Type[] types);
    }
}
