using System;
using System.Net;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public interface IAsyncUdpClient
    {
        Task<bool> ReceiveAllAsync(byte[] buffer, int count, EndPoint endPoint);
        Task<Tuple<int, EndPoint>> ReceiveAsync(byte[] buffer, int offset, int count, EndPoint endPoint);
        Task<bool> SendAllAsync(byte[] buffer, int count, EndPoint endPoint);
        Task<Tuple<int, EndPoint>> SendToAsync(byte[] buffer, int offset, int count, EndPoint endPoint);
        void Bind(EndPoint endPoint);
    }
}