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

        public int HeaderLength { get; private set; } = 0x04;

        public int BodyLength { get; private set; }

        protected Socks5ConnectionBase()
        {
        }

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

        public virtual byte[] SerializeHeader()
            => new byte[]
            {
                Version,
                0x00,
                Reserved,
                (byte) AddressType
            };

        public byte[] SerializeBody()
            => DestinationAddress
                .Concat(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(DestinationPort)))
                .ToArray();

        public virtual void DeserializeHeader(IList<byte> serialized)
        {
            if (Version != serialized[0])
                throw new ArgumentException("Data is invalid, Version does not match", nameof(serialized));
            if (Reserved != serialized[2])
                throw new ArgumentException("Data is invalid, Reserved does not match", nameof(serialized));

            AddressType = (SocksAddressType)serialized[3];
            BodyLength = 2;                             // the port
            switch (AddressType)
            {
                case SocksAddressType.Domain:
                    HeaderLength = 0x05;
                    BodyLength += serialized[4];
                    break;
                case SocksAddressType.IPv4:
                    BodyLength += 0x04;
                    break;
                case SocksAddressType.IPv6:
                    BodyLength += 0x10;
                    break;

                default:
                    throw new ArgumentException("Data is invalid, AddressType is invalid", nameof(serialized));
            }
        }

        public void DeserializeBody(IList<byte> serialized)
        {
            DestinationAddress = serialized.Take(BodyLength - 2).ToArray();

            var portBuf = new[] {serialized[BodyLength - 2], serialized[BodyLength - 1]};
            var hostPort = BitConverter.ToInt16(portBuf, 0);
            DestinationPort = IPAddress.NetworkToHostOrder(hostPort);
        }
    }
}
