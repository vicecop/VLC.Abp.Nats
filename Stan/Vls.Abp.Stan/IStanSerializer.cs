using System;

namespace Vls.Abp.Stan
{
    public interface IStanSerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] value, Type type);
        T Deserialize<T>(byte[] value);
    }
}
