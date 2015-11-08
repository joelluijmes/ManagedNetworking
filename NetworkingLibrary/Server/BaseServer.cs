using NetworkingLibrary.Client;
using System.Net.Sockets;

namespace NetworkingLibrary.Server
{
    public abstract class BaseServer
    {
        protected abstract Socket AcceptClient();

        protected T CreateClient<T>(Socket socket) where T : BaseClient, ITcpClient
            => BaseClient.CreateClient<T>(socket);

        public virtual T AcceptClient<T>() where T : BaseClient, ITcpClient
            => CreateClient<T>(AcceptClient());
    }
}
