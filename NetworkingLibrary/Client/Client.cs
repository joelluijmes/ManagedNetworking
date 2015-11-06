using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public class Client : IClient
    {
        protected readonly Socket _socket;

        public virtual EndPoint RemoteEndPoint => _socket?.RemoteEndPoint;
        public virtual EndPoint LocalEndPoint => _socket?.LocalEndPoint;

        public Client() : this(ProtocolType.Tcp) { }

        public Client(ProtocolType type)
        {
            if (type == ProtocolType.Udp)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, type);
            else if (type == ProtocolType.Tcp)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
            else
                throw new NotImplementedException("Unable to instantiate non tcp/udp client.");
        }

        internal Client(Socket socket)
        {
            _socket = socket;
        }

        public void Bind(EndPoint endPoint)
        {
            _socket.Bind(endPoint);
        }

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

        public virtual int SendTo(byte[] buffer, int offset, int count, EndPoint endPoint)
            => _socket.SendTo(buffer, offset, count, SocketFlags.None, endPoint);

        public virtual bool SendAll(byte[] buffer, int count)
            => TransceiveAll(Send, buffer, count);

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
