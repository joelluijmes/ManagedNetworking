using NetworkingLibrary.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetworkingLibrary.Serializable
{
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

        public static Task<T> ReceiveSerializable<T>(this IAsyncTcpClient client) where T : ISerializable, new() =>
            ReceiveSerializableImpl<T>(client.ReceiveAsync);

        public static Task<T> ReceiveFromSerializable<T>(this IAsyncUdpClient client, EndPoint endPoint) where T : ISerializable, new() =>
            ReceiveSerializableImpl<T>(async (buf, offset, count) => (await client.ReceiveAsync(buf, offset, count, endPoint)).Item1);

        public static Task<bool> SendSerializable(this IAsyncTcpClient client, ISerializable serializable)
        {
            var buffer = serializable.Serialize();
            return client.SendAllAsync(buffer, buffer.Length);
        }

        public static Task<bool> SendToSerializable(this IAsyncUdpClient client, ISerializable serializable, EndPoint endPoint)
        {
            var buffer = serializable.Serialize();
            return client.SendAllAsync(buffer, buffer.Length, endPoint);
        }

        private static async Task<T> ReceiveSerializableImpl<T>(Func<byte[], int, int, Task<int>> transfer) where T : ISerializable, new()
        {
            var serializable = new T();

            var oldHeaderLength = serializable.HeaderLength;
            var bufHeader = new byte[oldHeaderLength];
            
            if (await transfer(bufHeader, 0, bufHeader.Length) == 0)            // TODO: probably receive all
                return default(T);
            serializable.DeserializeHeader(bufHeader);

            if (serializable.BodyLength == 0)                                   // no body
                return serializable;

            var delta = oldHeaderLength - serializable.HeaderLength;
            if (delta < 0)                                                      // Header shrinked -> contained body data
                throw new InvalidOperationException("The header cannot grow because that would mean it wasn't fully deserialized yet.");

            var bufBody = new byte[serializable.BodyLength + delta];
            if (delta > 0)                                                      // First part of the body was already in the header
                Buffer.BlockCopy(bufHeader, serializable.HeaderLength, bufBody, 0, delta);

            var received = 0;
            while (received < serializable.BodyLength - delta)                  // Remaining part of the body
            {
                var current = await transfer(bufBody, delta + received, serializable.BodyLength - received);
                if (current == 0)                                               // Disconnected
                    return default(T);

                received += current;
            }
            serializable.DeserializeBody(bufBody);

            return serializable;
        }
    }
}