using System.Net;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal sealed class Socks5ConnectionResponse : Socks5ConnectionBase
    {
        public SocksResponseStatus Status { get; private set; }

        public Socks5ConnectionResponse(SocksResponseStatus status, IPAddress destination, int port) : base(destination, port)
        {
            Status = status;
        }

        public Socks5ConnectionResponse(SocksResponseStatus status, string domain, int port = 80) : base(domain, port)
        {
            Status = status;
        }

        public override byte[] Serialize()
        {
            var buffer = base.Serialize();
            buffer[1] = (byte)Status;

            return buffer;
        }

        public override void Deserialize(byte[] serialized)
        {
            Status = (SocksResponseStatus) serialized[1];
            base.Deserialize(serialized);
        }
    }
}
