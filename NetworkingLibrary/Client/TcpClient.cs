using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Client
{
    public sealed partial class TcpClient : BaseClient, ITcpClient
    {
        public TcpClient() : base(ProtocolType.Tcp) { }
        internal TcpClient(Socket socket) : base(socket) { }

        public bool EventOnDisconnect { get; set; } = true;

        private delegate int SocketTransferFunc(byte[] buffer, int offset, int count, SocketFlags socketFlags, out SocketError socketError);
        private delegate int TransferFunc(byte[] buffer, int offset, int count);
        
        public bool Connect(EndPoint endPoint)
        {
            try
            {
                Socket.Connect(endPoint);
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

        public void Disconnect(bool createNewSocket = true)
        {
            if (Socket == null)
                return;

            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();

            if (!createNewSocket)
                return;

            Socket.Dispose();
            CreateSocket();
        }

        public int Send(byte[] buffer, int offset = 0, int count = -1)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return Transfer(Socket.Send, buffer, offset, count);
        }

        public bool SendAll(byte[] buffer, int count = -1)
        {
            ValidateTransferAllArguments(buffer, ref count);
            return TransferAll(Send, buffer, count);
        }

        public int Receive(byte[] buffer, int offset = 0, int count = -1)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return Transfer(Socket.Receive, buffer, offset, count);
        }

        public bool ReceiveAll(byte[] buffer, int count = -1)
        {
            ValidateTransferAllArguments(buffer, ref count);
            return TransferAll(Receive, buffer, count);
        }

        private int Transfer(SocketTransferFunc func, byte[] buffer, int offset, int count)
        {
            SocketError error;
            var transfered = func(buffer, offset, count, SocketFlags.None, out error);

            // TODO: Add error handling

            switch (error)
            {
                case SocketError.Success:
                    break;

                //case SocketError.ConnectionReset:   // disconnected :(
                //    Disconnected = true;
                //    break;

                default:
                    throw new InvalidOperationException("Unhandled socket error!", new SocketException((int)error));
            }

            return transfered;
        }

        private static bool TransferAll(TransferFunc func, byte[] buffer, int count)
        {
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
