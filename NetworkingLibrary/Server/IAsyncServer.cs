using NetworkingLibrary.Client;
using System.Threading.Tasks;

namespace NetworkingLibrary.Server
{
    public interface IAsyncServer
    {
        Task<TcpClient> AcceptClientAsync();
    }
}