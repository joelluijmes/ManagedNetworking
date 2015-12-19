using System;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Server
{
    public partial class Server : IEventServer
    {
        public event EventHandler<ClientEventArgs> ClientConnected;

        public void BeginAccept()
        {
            _socket.BeginAccept((result) =>
            {
                var clientSocket = _socket.EndAccept(result);
                var client = new TcpClient(clientSocket);

                ClientConnected?.Invoke(this, new ClientEventArgs(client));
            }, null);
        }
    }
}
