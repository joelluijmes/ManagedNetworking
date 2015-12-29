using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (serializable.BodyLength == 0) // no body
            {
                Debug.WriteLine($"[WARN] BodyLength is 0 with HeaderLength is {serializable.HeaderLength} ({serializable.GetType().Name})");
                return buf1;
            }

            var buf2 = serializable.SerializeBody();
            
            return buf1.Concat(buf2).ToArray();
        }

        public static void Deserialize(this ISerializable serializable, byte[] serialized)
        {
            // TODO: Error Handling??
            serializable.DeserializeHeader(serialized);

            if (serializable.BodyLength == 0)                                   // no body
                return;

            var segment = new ArraySegment<byte>(serialized, serializable.HeaderLength, serializable.BodyLength);
            serializable.DeserializeBody(segment);
        }

        public static async Task<T> ReceiveSerializable<T>(this TcpClient client) where T : ISerializable, new()
        {
            var serializable = new T();

            var oldHeaderLength = serializable.HeaderLength;
            var bufHeader = new byte[oldHeaderLength];
            if (!await client.ReceiveAllAsync(bufHeader))
                return default(T);
            serializable.DeserializeHeader(bufHeader);

            if (serializable.BodyLength == 0)                                   // no body
                return serializable;

            var delta = oldHeaderLength - serializable.HeaderLength;
            if (delta < 0)                                                      // Header shrinked -> contained body data
                throw new InvalidOperationException("The header cannot grow because that would mean it wasn't fully deserialized yet.");

            if (delta == 0)
            {
                var buffer = new byte[serializable.BodyLength];
                if (!await client.ReceiveAllAsync(buffer, buffer.Length))
                    return default(T);
                serializable.DeserializeBody(buffer);

                return serializable;
            }

            var bufBody = new byte[serializable.BodyLength + delta];
            Buffer.BlockCopy(bufHeader, serializable.HeaderLength, bufBody, 0, delta);

            var received = 0;
            while (received < serializable.BodyLength - delta)
            {
                var current = await client.ReceiveAsync(bufBody, delta + received, serializable.BodyLength - received);
                if (current == 0)                                               // Disconnected
                    return default(T);

                received += current;
            }
            serializable.DeserializeBody(bufBody);

            return serializable;
        }

        private static async Task<bool> ReceiveHeader<T>(T serializable, IAsyncTcpClient client) where T : ISerializable, new()
        {
            var bufHeader = new byte[serializable.HeaderLength];
            if (!await client.ReceiveAllAsync(bufHeader, bufHeader.Length))
                return false;

            serializable.DeserializeHeader(bufHeader);
            return true;
        }
        
        public static Task<bool> SendSerializable(this TcpClient client, ISerializable serializable)
        {
            var buffer = serializable.Serialize();
            return client.SendAllAsync(buffer);
        }
    }
}
