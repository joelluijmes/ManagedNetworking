using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal sealed class Socks5GreetingRequest : ISerializable
    {
        public byte Version => 0x05;
        public byte MethodCount => (byte) Authentications.Count;
        public ReadOnlyCollection<SocksAuthentication> Authentications { get; private set; }

        public int HeaderLength => 0x02;
        public int BodyLength { get; private set; }

        public Socks5GreetingRequest()
        {
        }

        public Socks5GreetingRequest(IList<SocksAuthentication> authentications)
        {
            Authentications = new ReadOnlyCollection<SocksAuthentication>(authentications);
            BodyLength = authentications.Count;
        }

        public byte[] Serialize()
            => new[]
            {
                Version,
                MethodCount
            }
            .Concat(Authentications.Select(a => (byte) a))
            .ToArray();

        public void Deserialize(byte[] serialized)
        {
            if (Version != serialized[0])
                throw new ArgumentException("Data is invalid, Version does not match", nameof(serialized));

            var count = serialized[1];
            var methods = new SocksAuthentication[count];
            for (var i = 0; i < count; ++i)
                methods[i] = (SocksAuthentication) serialized[i + 2];

            Authentications = new ReadOnlyCollection<SocksAuthentication>(methods);
        }


        public byte[] SerializeHeader()
            => new[]
            {
                Version,
                MethodCount
            };

        public byte[] SerializeBody()
            => Authentications.Select(a => (byte)a).ToArray();

        public void DeserializeHeader(IList<byte> serialized)
        {
            if (Version != serialized[0])
                throw new ArgumentException("Data is invalid, Version does not match", nameof(serialized));

            BodyLength = serialized[1];
        }

        public void DeserializeBody(IList<byte> serialized)
        {
            var methods = Enumerable.Range(0, BodyLength).Select(i => (SocksAuthentication)i);
            Authentications = new ReadOnlyCollection<SocksAuthentication>(methods.ToList());
        }
    }
}
