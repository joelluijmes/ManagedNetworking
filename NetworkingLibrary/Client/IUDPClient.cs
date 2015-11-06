using System.Net;

namespace NetworkingLibrary.Client
{
    public interface IUdpClient
    {
        void Bind(EndPoint endPoint);
        int SendTo(byte[] buffer, int offset, int count, EndPoint endPoint);
        bool SendToAll(byte[] buffer, int count, EndPoint endPoint);
        bool SendToAll(byte[] buffer, EndPoint endPoint);
        int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint endPoint);
        bool ReceiveFromAll(byte[] buffer, int count, ref EndPoint endPoint);
        bool ReceiveFromAll(byte[] buffer, ref EndPoint endPoint);
    }
}
