using System.Collections.Generic;
using System.Net;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal  sealed class Socks5ConnectionRequest : Socks5ConnectionBase
    {
        public SocksCommand Command { get; private set; }

        public Socks5ConnectionRequest()
        {
        }

        public Socks5ConnectionRequest(SocksCommand command, IPAddress destination, int port) : base(destination, port)
        {
            Command = command;
        }

        public Socks5ConnectionRequest(SocksCommand command, string domain, int port = 80) : base(domain, port)
        {
            Command = command;
        }

        public override byte[] SerializeHeader()
        {
            var buffer = base.SerializeHeader();
            buffer[1] = (byte)Command;

            return buffer;
        }
        
        public override void DeserializeHeader(IList<byte> serialized)
        {
            Command = (SocksCommand) serialized[1];
            base.DeserializeHeader(serialized);
        }
    }
}
