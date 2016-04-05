using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public sealed partial class UdpClient : BaseClient
    {
        private delegate int TransferFunc(byte[] buffer, int offset, int count);

        public UdpClient() : base(ProtocolType.Udp)
        { }

        public UdpClient(Socket socket) : base(socket)
        { }

        public int SendTo(byte[] buffer, int offset, int count, EndPoint remoteEndPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return Socket.SendTo(buffer, offset, count, SocketFlags.None, remoteEndPoint);
        }
        
        public int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint remoteEndPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return Socket.ReceiveFrom(buffer, offset, count, SocketFlags.None, ref remoteEndPoint);
        }

        public void Bind(EndPoint endPoint) 
        {
            Socket.Bind(endPoint);
        }
    }
}