using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public sealed partial class UdpClient : BaseClient, IUdpClient
    {
        public UdpClient() : base(ProtocolType.Udp)
        { }

        public UdpClient(Socket socket) : base(socket)
        { }

        public int SendTo(byte[] buffer, int offset, int count, EndPoint remoteEndPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return Socket.SendTo(buffer, offset, count, SocketFlags.None, remoteEndPoint);
        }

        public bool SendToAll(byte[] buffer, int offset, int count, EndPoint remoteEndPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            var transfered = 0;

            while (transfered < count)
            {
                var current = Socket.SendTo(buffer, transfered, count - transfered, SocketFlags.None, remoteEndPoint);
                if (current == 0)
                    return false;
                transfered += current;
            }

            return true;
        }
        
        public int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint remoteEndPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return Socket.ReceiveFrom(buffer, offset, count, SocketFlags.None, ref remoteEndPoint);
        }

        public bool ReceiveFromAll(byte[] buffer, int count, ref EndPoint remoteEndPoint)
        {
            ValidateTransferAllArguments(buffer, ref count);
            var transfered = 0;

            while (transfered < count)
            {
                var current = Socket.ReceiveFrom(buffer, transfered, count - transfered, SocketFlags.None, ref remoteEndPoint);
                if (current == 0)
                    return false;
                transfered += current;
            }

            return true;
        }

        public void Bind(EndPoint endPoint) 
        {
            Socket.Bind(endPoint);
        }
    }
}