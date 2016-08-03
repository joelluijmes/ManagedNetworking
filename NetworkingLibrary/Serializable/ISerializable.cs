using System.Collections.Generic;

namespace NetworkingLibrary.Serializable
{
    public interface ISerializable : IHeadSerializable
    {
        int BodyLength { get; }

        byte[] SerializeBody();
        void DeserializeBody(IList<byte> serialized);
    }
}
