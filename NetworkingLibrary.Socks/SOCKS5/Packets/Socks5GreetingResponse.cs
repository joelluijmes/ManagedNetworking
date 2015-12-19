using System;
using System.Collections.Generic;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal sealed class Socks5GreetingResponse : ISerializable
    {
        public byte Version => 0x05;
        public SocksAuthentication AuthenticationMethod { get; private set; }

        public int HeaderLength => 0x02;
        public int BodyLength => 0x00;

        public Socks5GreetingResponse(SocksAuthentication authentication)
        {
            AuthenticationMethod = authentication;
        }

        public byte[] Serialize()
            => new[]
            {
                Version,
                (byte) AuthenticationMethod
            };

        public void Deserialize(byte[] serialized)
        {
            if (Version != serialized[0])
                throw new ArgumentException("Data is invalid, Version does not match", nameof(serialized));

            AuthenticationMethod = (SocksAuthentication) serialized[1];
        }

        
        public byte[] SerializeHeader()
            => new[]
            {
                Version,
                (byte) AuthenticationMethod
            };

        public byte[] SerializeBody()
        {
            throw new NotImplementedException();
        }

        public void DeserializeHeader(IList<byte> serialized)
        {
            if (Version != serialized[0])
                throw new ArgumentException("Data is invalid, Version does not match", nameof(serialized));

            AuthenticationMethod = (SocksAuthentication)serialized[1];
        }

        public void DeserializeBody(IList<byte> serialized)
        {
            throw new NotImplementedException();
        }
    }
}
