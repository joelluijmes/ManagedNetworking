using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetworkingLibrary.Socks.Packets
{
    internal sealed class Socks5GreetingRequest
    {
        public byte Version => 0x05;
        public byte MethodCount => (byte) Authentications.Count;
        public ReadOnlyCollection<SocksAuthentication> Authentications { get; private set; }

        public Socks5GreetingRequest(IList<SocksAuthentication> authentications)
        {
            Authentications = new ReadOnlyCollection<SocksAuthentication>(authentications);
        }
    }
}
