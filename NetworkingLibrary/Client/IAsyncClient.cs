using System.Net;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public interface IAsyncClient
    {
        Task<bool> ConnectAsync(EndPoint endPoint);
        Task<bool> ReceiveAllAsync(byte[] buffer, int count);
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
        Task<bool> SendAllAsync(byte[] buffer, int count);
        Task<int> SendAsync(byte[] buffer, int offset, int count);
    }
}