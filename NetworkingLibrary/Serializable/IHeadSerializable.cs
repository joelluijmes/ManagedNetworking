using System.Collections.Generic;

namespace NetworkingLibrary.Serializable
{
    public interface IHeadSerializable
    {
        int HeaderLength { get; }

        void DeserializeHeader(IList<byte> serialized);
        byte[] SerializeHeader();
    }
}