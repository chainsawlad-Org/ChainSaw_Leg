using System;

namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveSerializer
    {
        byte[] Serialize<T>(T value);
        byte[] Serialize(object value, Type expectedType);
        T Deserialize<T>(byte[] data);
        object Deserialize(byte[] data, Type expectedType);
    }
}
