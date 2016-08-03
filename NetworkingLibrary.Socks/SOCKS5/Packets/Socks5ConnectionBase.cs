using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkingLibrary.Serializable;

namespace NetworkingLibrary.Socks.SOCKS5.Packets
{
    internal abstract class Socks5ConnectionBase : ISerializable
    {
        public byte Version => 0x05;
        public byte Reserved => 0x00;
        public SocksAddressType AddressType { get; protected set; }
        public byte[] DestinationAddress { get; protected set; }
        public short DestinationPort { get; protected set; }

        public int HeaderLength { get; private set; } = 0x05;           // it's a lie for IPv4 and IPv6 :$
        public int BodyLength { get; protected set; }

        public IPEndPoint EndPoint
        {
            get
            {
                IPAddress address;
                if (AddressType == SocksAddressType.Domain)
                {
                    var domain = Encoding.UTF8.GetString(DestinationAddress);
                    address = Dns.GetHostAddresses(domain).FirstOrDefault();
                    if (address == null)
                        throw new InvalidOperationException($"Couldn't resolve domain: {domain}");  // TODO: Exception
                }
                else
                {
                    address = new IPAddress(DestinationAddress);
                }

                return new IPEndPoint(address, DestinationPort);
            }
        }

        protected Socks5ConnectionBase()
        {
        }

        protected Socks5ConnectionBase(IPAddress destination, int port)
        {
            HeaderLength = 0x04;
            BodyLength = 0x02;
            switch (destination.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    AddressType = SocksAddressType.IPv4;
                    BodyLength += 0x04;
                    break;
                case AddressFamily.InterNetworkV6:
                    AddressType = SocksAddressType.IPv6;
                    BodyLength += 0x10;
                    break;
                default:
                    throw new ArgumentException("AddressFamily is not InterNetwork (IPv4) or InterNetworkV6 (IPv6)", nameof(destination));
            }

            DestinationAddress = destination.GetAddressBytes();
            DestinationPort = checked((short) port);
        }

        protected Socks5ConnectionBase(string domain, int port = 80)
        {
            AddressType = SocksAddressType.Domain;
            BodyLength = 0x02 + domain.Length;
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
                    BodyLength += serialized[4];
                    break;
                case SocksAddressType.IPv4:
                    HeaderLength = 0x04;
                    BodyLength += 0x04;
                    break;
                case SocksAddressType.IPv6:
                    HeaderLength = 0x04;
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
