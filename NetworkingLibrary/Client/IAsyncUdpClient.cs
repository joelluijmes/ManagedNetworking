using System;
using System.Net;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public interface IAsyncUdpClient
    {   
        Task<Tuple<int, EndPoint>> ReceiveFromAsync(byte[] buffer, int offset, int count, EndPoint endPoint);
        Task<Tuple<bool, EndPoint>> ReceiveAllFromAsync(byte[] buffer, int count, EndPoint endPoint);
        Task<Tuple<bool, EndPoint>> ReceiveAllFromAsync(byte[] buffer, EndPoint endPoint);
        Task<int> SendToAsync(byte[] buffer, int offset, int count, EndPoint endPoint);
        Task<bool> SendToAllAsync(byte[] buffer, int count, EndPoint endPoint);
        Task<bool> SendToAllAsync(byte[] buffer, EndPoint endPoint);
    }
}
