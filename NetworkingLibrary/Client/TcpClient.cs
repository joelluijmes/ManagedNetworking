using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public class TcpClient : BaseClient, ITcpClient
    {
        public TcpClient() : base(ProtocolType.Tcp) { }
        
        public virtual bool Connect(EndPoint endPoint)
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

        public virtual bool ConnectAny(IEnumerable<EndPoint> endPoints, out EndPoint endPoint)
        {
            var connectedEndPoint = endPoints.Where(Connect).FirstOrDefault();
            endPoint = connectedEndPoint;

            return connectedEndPoint != default(EndPoint);
        }

        public virtual int Send(byte[] buffer, int offset, int count)
            => _socket.Send(buffer, offset, count, SocketFlags.None);
        
        public virtual bool SendAll(byte[] buffer, int count)
            => TransceiveAll(Send, buffer, count);
        
        public virtual int Receive(byte[] buffer, int offset, int count)
        {
            SocketError error;
            var received = _socket.Receive(buffer, offset, count, SocketFlags.None, out error);

            // TODO: Add some more handling?
            if (error != SocketError.Success)
                throw new SocketException((int)error);

            // recv returns 0 -> gracefully shutdown
            if (received == 0)
                _socket.Shutdown(SocketShutdown.Both);

            return received;
        }
        
        public virtual bool ReceiveAll(byte[] buffer, int count)
            => TransceiveAll(Receive, buffer, count);
        
        private bool TransceiveAll(Func<byte[], int, int, int> func, byte[] buffer, int count)
        {
            var total = 0;
            while (total < count)
            {
                var current = func(buffer, total, count - total);
                if (current == 0)
                    return false;

                total += current;
            }

            return true;
        }
    }
}
