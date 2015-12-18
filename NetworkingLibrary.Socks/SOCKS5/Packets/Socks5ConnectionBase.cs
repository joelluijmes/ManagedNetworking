using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal abstract class Socks5ConnectionBase : ISerializable
    {
        public byte Version => 0x05;
        public byte Reserved => 0x00;
        public SocksAddressType AddressType { get; private set; }
        public byte[] DestinationAddress { get; private set; }
        public short DestinationPort { get; private set; }

        protected Socks5ConnectionBase(IPAddress destination, int port)
        {
            AddressType = destination.AddressFamily == AddressFamily.InterNetwork
                ? SocksAddressType.IPv4
                : SocksAddressType.IPv6;
            DestinationAddress = destination.GetAddressBytes();
            DestinationPort = checked((short) port);
        }

        protected Socks5ConnectionBase(string domain, int port = 80)
        {
            AddressType = SocksAddressType.Domain;
            DestinationAddress = new byte[domain.Length + 1];
            DestinationAddress[0] = (byte)domain.Length;
            var buf = Encoding.UTF8.GetBytes(domain);
            Buffer.BlockCopy(buf, 0, DestinationAddress, 1, buf.Length);
            DestinationPort = checked((short) port);
        }

        public virtual byte[] Serialize()
            => new byte[]
            {
                Version,
                0x00,
                Reserved,
                (byte) AddressType
            }
            .Concat(DestinationAddress)
            .Concat(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(DestinationPort)))
            .ToArray();

        public virtual void Deserialize(byte[] serialized)
        {
            if (Version != serialized[0])
                throw new ArgumentException("Data is invalid, Version does not match", nameof(serialized));
            if (Reserved != serialized[2])
                throw new ArgumentException("Data is invalid, Reserved does not match", nameof(serialized));

            AddressType = (SocksAddressType) serialized[3];

            int portOffset;
            switch (AddressType)
            {
                case SocksAddressType.Domain:
                {
                    var len = serialized[4];
                    DestinationAddress = serialized.Skip(5).Take(len).ToArray();
                    portOffset = len  + 5;

                    break;
                }
                case SocksAddressType.IPv4:
                    DestinationAddress = serialized.Skip(4).Take(4).ToArray();
                    portOffset = 8;

                    break;
                case SocksAddressType.IPv6:
                    DestinationAddress = serialized.Skip(4).Take(16).ToArray();
                    portOffset = 20;

                    break;

                default:
                    throw new ArgumentException("Data is invalid, AddressType is invalid", nameof(serialized));
            }

            var hostPort = BitConverter.ToInt16(serialized, portOffset);
            DestinationPort = IPAddress.NetworkToHostOrder(hostPort);
        }
    }
}
