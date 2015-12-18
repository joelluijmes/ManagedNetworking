using System.Net;

namespace NetworkingLibrary.Socks.Packets
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
    }
}
