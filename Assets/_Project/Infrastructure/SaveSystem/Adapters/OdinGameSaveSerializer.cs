// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;
using OdinSerializer;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class OdinGameSaveSerializer : IGameSaveSerializer
    {
        public byte[] Serialize<T>(T value)
        {
            return Serialize(value, typeof(T));
        }

        public byte[] Serialize(object value, Type expectedType)
        {
            try
            {
                if (value == null || expectedType == null || !expectedType.IsInstanceOfType(value))
                    throw new InvalidOperationException("Value does not match the expected save DTO type.");

                return SerializationUtility.SerializeValueWeak(
                    value,
                    DataFormat.Binary,
                    (SerializationContext)null);
            }
            catch (Exception exception) when (exception is not GameSaveSerializationException)
            {
                throw new GameSaveSerializationException("Failed to serialize game save data.", exception);
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(data, typeof(T));
        }

        public object Deserialize(byte[] data, Type expectedType)
        {
            try
            {
                if (data == null || data.Length == 0)
                    throw new InvalidOperationException("Serialized game save data is empty.");

                object value = SerializationUtility.DeserializeValueWeak(
                    data,
                    DataFormat.Binary,
                    (DeserializationContext)null);

                if (value == null || expectedType == null || !expectedType.IsInstanceOfType(value))
                    throw new InvalidOperationException("Serialized game save data has an unexpected type.");

                return value;
            }
            catch (Exception exception) when (exception is not GameSaveSerializationException)
            {
                throw new GameSaveSerializationException("Failed to deserialize game save data.", exception);
            }
        }
    }
}
