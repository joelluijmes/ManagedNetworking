using System;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal sealed class Socks5GreetingResponse : ISerializable
    {
        public byte Version => 0x05;
        public SocksAuthentication AuthenticationMethod { get; private set; }

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
    }
}
