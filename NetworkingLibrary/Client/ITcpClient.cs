using System.Net;

namespace NetworkingLibrary.Client
{
    public interface ITcpClient
    {
        bool Connect(EndPoint endPoint);
        int Receive(byte[] buffer, int offset, int count);
        bool ReceiveAll(byte[] buffer, int count);
        int Send(byte[] buffer, int offset, int count);
        bool SendAll(byte[] buffer, int count);
    }
}