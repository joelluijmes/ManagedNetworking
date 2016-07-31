using System.Net;

namespace NetworkingLibrary.Client
{
    public interface IUdpClient
    {
        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }

        int SendTo(byte[] buffer, int offset, int count, EndPoint remoteEndPoint);
        bool SendToAll(byte[] buffer, int offset, int count, EndPoint remoteEndPoint);
        int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint remoteEndPoint);
        bool ReceiveFromAll(byte[] buffer, int count, ref EndPoint remoteEndPoint);
        void Bind(EndPoint endPoint);
    }
}