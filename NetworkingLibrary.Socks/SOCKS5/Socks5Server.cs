using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Server;
using NetworkingLibrary.Socks.SOCKS5.Packets;

using TcpClientEventArgs = NetworkingLibrary.Events.ClientEventArgs;
using SocksClientEventArgs = NetworkingLibrary.Socks.Events.ClientEventArgs;

namespace NetworkingLibrary.Socks.SOCKS5
{
    public sealed class Socks5Server
    {
        private readonly TcpServer _server;
        private readonly List<Socks5Client> _clients;

        public event EventHandler<SocksClientEventArgs> ClientConnected;
        public event EventHandler<SocksClientEventArgs> ClientDisconnected;
        public event EventHandler<TcpClientEventArgs> InvalidClientConnected;
        

        public Socks5Server(int port)
        {
            _server = new TcpServer();
            _server.Listen(port);
            _server.ClientConnected += ServerOnClientConnected;

            _clients = new List<Socks5Client>();

            _server.BeginAccept();
        }

        private async void ServerOnClientConnected(object sender, TcpClientEventArgs e)
        {
            _server.BeginAccept();

            var client = new Socks5Client(e.Client);
            client.InternalClient.ClientDisconnected += (o, args) 
                => ClientDisconnected?.Invoke(this, new SocksClientEventArgs(client));

            if (await HandleGreeting(client))
                ClientConnected?.Invoke(this, new SocksClientEventArgs(client));
            else
                InvalidClientConnected?.Invoke(this, new TcpClientEventArgs(e.Client));

            await HandleConnectionInitiate(client);
            client.StartTunneling();

            _clients.Add(client);
        }

        private static async Task<bool> HandleConnectionInitiate(Socks5Client client)
        {
            var connectionRequest = await client.InternalClient.ReceiveSerializable<Socks5ConnectionRequest>();
            if (connectionRequest == null)
                return false;

            // TODO: Add validation etc.
            var success = await client.ConnectWith(connectionRequest.EndPoint);
            var status = success ? SocksResponseStatus.OK : SocksResponseStatus.GeneralFailure;
            
            var connectionResponse = new Socks5ConnectionResponse(status, connectionRequest);
            return await client.InternalClient.SendSerializable(connectionResponse);
        }

        private static async Task<bool> HandleGreeting(Socks5Client client)
        {
            // TODO: Add state or something?
            var greetingRequest = await client.InternalClient.ReceiveSerializable<Socks5GreetingRequest>();
            if (greetingRequest == null)
                return false;

            // TODO: Authentication handling
            var greetingResponse = new Socks5GreetingResponse(SocksAuthentication.NoAuthentication);
            return await client.InternalClient.SendSerializable(greetingResponse);
        }
    }
}
