using System.Net;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal  sealed class Socks5ConnectionRequest : Socks5ConnectionBase
    {
        public SocksCommand Command { get; private set; }

        public Socks5ConnectionRequest(SocksCommand command, IPAddress destination, int port) : base(destination, port)
        {
            Command = command;
        }

        public Socks5ConnectionRequest(SocksCommand command, string domain, int port = 80) : base(domain, port)
        {
            Command = command;
        }

        public override byte[] Serialize()
        {
            var buffer = base.Serialize();
            buffer[1] = (byte) Command;

            return buffer;
        }

        public override void Deserialize(byte[] serialized)
        {
            Command = (SocksCommand) serialized[1];
            base.Deserialize(serialized);
        }
    }
}
