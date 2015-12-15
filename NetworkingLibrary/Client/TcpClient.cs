using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public sealed partial class TcpClient : BaseClient, ITcpClient
    {
        public TcpClient() : base(ProtocolType.Tcp) { }
        internal TcpClient(Socket socket) : base(socket) { }

        private delegate int SocketTransferFunc(byte[] buffer, int offset, int count, SocketFlags socketFlags, out SocketError socketError);
        private delegate int TransferFunc(byte[] buffer, int offset, int count);
        
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

        public int Send(byte[] buffer, int offset = 0, int count = -1)
            => Transfer(_socket.Send, buffer, offset, count);

        public bool SendAll(byte[] buffer, int count = -1)
            => TransferAll(Send, buffer, count);
        
        public int Receive(byte[] buffer, int offset = 0, int count = -1)
            => Transfer(_socket.Receive, buffer, offset, count);

        public bool ReceiveAll(byte[] buffer, int count = -1)
            => TransferAll(Receive, buffer, count);

        private static int Transfer(SocketTransferFunc func, byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (offset < 0)
                throw new ArgumentException("Offset must be positive.", nameof(offset));
            if (count == -1)
                count = buffer.Length;
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            SocketError error;
            var transfered = func(buffer, offset, count, SocketFlags.None, out error);

            // TODO: Add error handling
            if (error != SocketError.Success)
                throw new SocketException((int)error);

            return transfered;
        }

        private static bool TransferAll(TransferFunc func, byte[] buffer, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (count == -1)
                count = buffer.Length;
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            var transfered = 0;
            while (transfered < count)
            {
                var current = func(buffer, transfered, count - transfered);
                if (current == 0)
                    // TODO: Error or..?
                    return false;

                transfered += current;
            }

            return true;
        }
    }
}
