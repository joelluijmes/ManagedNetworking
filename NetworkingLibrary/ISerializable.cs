using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetworkingLibrary.Client;

namespace NetworkingLibrary
{
    public interface ISerializable
    {
        int HeaderLength { get; }
        int BodyLength { get; }

        byte[] SerializeHeader();
        byte[] SerializeBody();
        void DeserializeHeader(IList<byte> serialized);
        void DeserializeBody(IList<byte> serialized);
    }

    public static class SerializeableExtensions
    {
        public static byte[] Serialize(this ISerializable serializable)
        {
            // TODO: Error Handling??
            var buf1 = serializable.SerializeHeader();
            if (serializable.BodyLength == 0)           // no body
                return buf1;

            var buf2 = serializable.SerializeBody();
            
            return buf1.Concat(buf2).ToArray();
        }

        public static void Deserialize(this ISerializable serializable, byte[] serialized)
        {
            // TODO: Error Handling??
            serializable.DeserializeHeader(serialized);

            if (serializable.BodyLength == 0)           // no body
                return;

            var segment = new ArraySegment<byte>(serialized, serializable.HeaderLength, serializable.BodyLength);
            serializable.DeserializeBody(segment);
        }

        public static async Task<T> ReceiveSerializable<T>(this TcpClient client) where T : ISerializable, new()
        {
            var serializable = new T();

            var bufHeader = new byte[serializable.HeaderLength];
            if (!await client.ReceiveAllAsync(bufHeader))
                return default(T);

            if (serializable.BodyLength == 0) // no body
                return serializable;

            var bufBody = new byte[serializable.BodyLength];
            return await client.ReceiveAllAsync(bufBody)
                ? serializable
                : default(T);
        }

        public static Task<bool> SendSerializable(this TcpClient client, ISerializable serializable)
        {
            var buffer = serializable.Serialize();
            return client.SendAllAsync(buffer);
        }
    }
}
