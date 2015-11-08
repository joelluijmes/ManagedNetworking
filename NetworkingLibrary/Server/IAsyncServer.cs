using NetworkingLibrary.Client;
using System.Threading.Tasks;

namespace NetworkingLibrary.Server
{
    public interface IAsyncServer
    {
        Task<T> AcceptClientAsync<T>() where T : BaseClient, ITcpClient;
    }
}