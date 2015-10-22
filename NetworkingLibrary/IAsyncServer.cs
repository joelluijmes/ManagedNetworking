using System.Threading.Tasks;

namespace NetworkingLibrary
{
    public interface IAsyncServer
    {
        Task<T> AcceptClientAsync<T>();
    }
}