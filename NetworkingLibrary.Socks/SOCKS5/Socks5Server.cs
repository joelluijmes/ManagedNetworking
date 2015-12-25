using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;
using NetworkingLibrary.Server;

namespace NetworkingLibrary.Socks.SOCKS5
{
    public sealed class Socks5Server
    {
        private readonly TcpServer _server;
        private readonly List<Socks5Client> _clients; 

        public Socks5Server(int port)
        {
            _server = new TcpServer();
            _server.Listen(port);
            _server.ClientConnected += ServerOnClientConnected;

            _clients = new List<Socks5Client>();

            _server.BeginAccept();
        }

        private void ServerOnClientConnected(object sender, ClientEventArgs e)
        {
            var client = new Socks5Client(e.Client);
            _clients.Add(client);

            _server.BeginAccept();
        }
    }
}
