using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Socks.Packets
{
    internal abstract class Socks5ConnectionBase
    {
        public byte Version => 0x05;
        public byte Reserved => 0x00;
        public AddressType AddressType { get; private set; }
        public byte[] DestinationAddress { get; private set; }
        public short DestinationPort { get; private set; }

        protected Socks5ConnectionBase(IPAddress destination, int port)
        {
            AddressType = destination.AddressFamily == AddressFamily.InterNetwork
                ? AddressType.IPv4
                : AddressType.IPv6;
            DestinationAddress = destination.GetAddressBytes();
            DestinationPort = (short)IPAddress.HostToNetworkOrder(port);
        }

        protected Socks5ConnectionBase(string domain, int port = 80)
        {
            AddressType = AddressType.Domain;
            DestinationAddress = new byte[domain.Length + 1];
            DestinationAddress[0] = (byte)domain.Length;
            var buf = Encoding.UTF8.GetBytes(domain);
            Buffer.BlockCopy(buf, 0, DestinationAddress, 1, buf.Length);

            DestinationPort = (short)IPAddress.HostToNetworkOrder(port);
        }
    }
}
