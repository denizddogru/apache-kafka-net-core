using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace Kafka.Producer;
internal class CustomValueSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        // object -> json -> byte

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, typeof(T)));
    }
}
