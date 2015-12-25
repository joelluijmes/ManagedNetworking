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
        private const int RECV_BUFSIZE = 1024;

        internal TcpClient InternalClient { get; }
        private TcpClient _endPointClient;
        
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

        internal Task<bool> ConnectWithEndPoint(EndPoint endPoint)
        {
            _endPointClient = new TcpClient();
            return _endPointClient.ConnectAsync(endPoint);
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

        public void StartTunneling()
        {
            _endPointClient.ReceiveCompleted += OnRemoteReceived;
            _endPointClient.SendCompleted += (sender, args) =>
                Console.WriteLine($"[TO REMOTE] Sent {args.Count}");
            InternalClient.ReceiveCompleted += OnProxiedReceived;
            InternalClient.SendCompleted += (sender, args) =>
                Console.WriteLine($"[TO PROXIE] Sent {args.Count}");

            _endPointClient.BeginReceive(new byte[RECV_BUFSIZE]);
            InternalClient.BeginReceive(new byte[RECV_BUFSIZE]);
        }

        private void OnRemoteReceived(object sender, TransferEventArgs e)
        {
            // TODO: Disconnect??
            if (e.Count == 0) // disconnected
                return;

            _endPointClient.BeginReceive(new byte[RECV_BUFSIZE]);
            InternalClient.BeginSendAll(e.Bytes, e.Count);
        }

        private void OnProxiedReceived(object sender, TransferEventArgs e)
        {
            // TODO: Disconnect??
            if (e.Count == 0) // disconnected
                return;

            InternalClient.BeginReceive(new byte[RECV_BUFSIZE]);
            _endPointClient.BeginSendAll(e.Bytes, e.Count);
        }
    }
}
