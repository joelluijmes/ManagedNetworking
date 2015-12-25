using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;
using NetworkingLibrary.Socks.SOCKS5.Packets;

namespace NetworkingLibrary.Socks.SOCKS5
{
    public sealed class Socks5Client
    {
        internal TcpClient InternalClient { get; }
        
        public Socks5Client()
        {
            InternalClient = new TcpClient();
        }

        internal Socks5Client(TcpClient client)
        {
            InternalClient = client;
        }

        public async Task<bool> ConnectWithServer(EndPoint serverEndPoint)
        {
            var connected = await InternalClient.ConnectAsync(serverEndPoint);
            if (!connected)
                return false;

            return await InitiateGreeting();
        }

        private async Task<bool> InitiateGreeting()
        {
            var request = new Socks5GreetingRequest(new [] {SocksAuthentication.NoAuthentication});
            var result = await InternalClient.SendSerializable(request);
            if (!result)        // TODO: Error HANDLING
                throw new InvalidOperationException("Something went wrong when sending serialization");

            var greetingResponse = await InternalClient.ReceiveSerializable<Socks5GreetingResponse>();
            return greetingResponse.AuthenticationMethod == SocksAuthentication.NoAuthentication;
        }

        public async Task<SocksResponseStatus> InitiateConnection(string domain, int port = 80)
        {
            var request = new Socks5ConnectionRequest(SocksCommand.Connect, domain, port);
            var result = await InternalClient.SendSerializable(request);
            if (!result)        // TODO: Error HANDLING
                throw new InvalidOperationException("Something went wrong when sending serialization");

            var connectionResponse = await InternalClient.ReceiveSerializable<Socks5ConnectionResponse>();
            return connectionResponse.Status;
        }
    }
}
