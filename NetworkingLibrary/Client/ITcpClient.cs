using System.Net;

namespace NetworkingLibrary.Client
{
    public interface ITcpClient
    {
        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }


        bool Connect(EndPoint endPoint);
        int Receive(byte[] buffer, int offset, int count);
        bool ReceiveAll(byte[] buffer, int count);
        int Send(byte[] buffer, int offset, int count);
        bool SendAll(byte[] buffer, int count);
    }
}