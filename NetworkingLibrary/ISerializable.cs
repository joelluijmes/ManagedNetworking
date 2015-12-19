using System;
using System.Collections.Generic;
using System.Linq;

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
            var buf2 = serializable.SerializeBody();
            
            return buf1.Concat(buf2).ToArray();
        }

        public static void Deserialize(this ISerializable serializable, byte[] serialized)
        {
            // TODO: Error Handling??
            serializable.DeserializeHeader(serialized);

            var segment = new ArraySegment<byte>(serialized, serializable.HeaderLength, serializable.BodyLength);
            serializable.DeserializeBody(segment);
        }
    }
}
