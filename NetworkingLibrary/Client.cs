using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary
{
    public class Client : IClient
    {
        protected readonly Socket _socket;

        public Client()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        internal Client(Socket socket)
        {
            _socket = socket;
        }

        public bool Connect(EndPoint endPoint)
        {
            try
            {
                _socket.Connect(endPoint);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ConnectAny(IEnumerable<EndPoint> endPoints, out EndPoint endPoint)
        {
            var connectedEndPoint = endPoints.Where(Connect).FirstOrDefault();
            endPoint = connectedEndPoint;

            return connectedEndPoint != default(EndPoint);
        }

        public int Send(byte[] buffer, int offset, int count)
            => _socket.Send(buffer, offset, count, SocketFlags.None);

        public bool SendAll(byte[] buffer, int count)
        {
            var totalSent = 0;

            while (totalSent < count)
            {
                var sent = Send(buffer, totalSent, count - totalSent);
                if (sent == 0)
                    return false;

                totalSent += sent;
            }

            return true;
        }

        public int Receive(byte[] buffer, int offset, int count)
            => _socket.Receive(buffer, offset, count, SocketFlags.None);

        public bool ReceiveAll(byte[] buffer, int count)
        {
            var totalReceived = 0;

            while (totalReceived < count)
            {
                var received = Receive(buffer, totalReceived, count - totalReceived);
                if (received == 0)
                    return false;

                totalReceived += received;
            }

            return true;
        }
    }
}
