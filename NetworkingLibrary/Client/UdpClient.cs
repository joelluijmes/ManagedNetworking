using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public class UdpClient : BaseClient, IUdpClient
    {
        public UdpClient() : base(ProtocolType.Udp) { }

        public void Bind(EndPoint endPoint)
        {
            _socket.Bind(endPoint);
        }

        public virtual int SendTo(byte[] buffer, int offset, int count, EndPoint endPoint)
            => _socket.SendTo(buffer, offset, count, SocketFlags.None, endPoint);

        public virtual bool SendToAll(byte[] buffer, int count, EndPoint endPoint)
        {
            var total = 0;
            while (total < count)
            {
                var current = _socket.SendTo(buffer, total, count - total, SocketFlags.None, endPoint);
                if (current == 0)
                    return false;

                total += current;
            }

            return true;
        }

        public virtual int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint endPoint)
            => _socket.ReceiveFrom(buffer, offset, count, SocketFlags.None, ref endPoint);

        public virtual bool ReceiveFromAll(byte[] buffer, int count, ref EndPoint endPoint)
        {
            var total = 0;
            while (total < count)
            {
                var current = _socket.ReceiveFrom(buffer, total, count - total, SocketFlags.None, ref endPoint);
                if (current == 0)
                    return false;

                total += current;
            }

            return true;
        }
    }
}
