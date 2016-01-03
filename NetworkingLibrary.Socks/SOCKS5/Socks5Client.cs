using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;
using NetworkingLibrary.Socks.SOCKS5.Packets;

namespace NetworkingLibrary.Socks.SOCKS5
{
    public sealed partial class Socks5Client 
    {
        private const int RECV_BUFSIZE = UInt16.MaxValue;
        private readonly TaskCompletionSource<bool> _connectedCompletion;

        internal TcpClient InternalClient { get; }
        private TcpClient _endPointClient;

        public bool ConnectedWithSocksServer => _connectedCompletion?.Task.IsCompleted == true && _connectedCompletion.Task.Result;

        public Socks5Client(EndPoint socksServer)
        {
            InternalClient = new TcpClient();
            _connectedCompletion = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                var result = await ConnectWithSocksServer(socksServer);
                _connectedCompletion.SetResult(result);
            });
        }

        internal Socks5Client(TcpClient client)
        {
            InternalClient = client;

            _connectedCompletion = new TaskCompletionSource<bool>();
            _connectedCompletion.SetResult(true);
        }

        public async Task<SocksResponseStatus> InitiateConnection(string domain, int port = 80)
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Domain cannot be null or whitespace.", nameof(domain));
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port), "Port number is invalid");

            if (!await _connectedCompletion.Task)
                return SocksResponseStatus.GeneralFailure;

            var request = new Socks5ConnectionRequest(SocksCommand.Connect, domain, port);
            var result = await InternalClient.SendSerializable(request);
            if (!result)        // TODO: Error HANDLING
                throw new InvalidOperationException("Something went wrong when sending serialization");

            var connectionResponse = await InternalClient.ReceiveSerializable<Socks5ConnectionResponse>();
            return connectionResponse.Status;
        }

        public async Task<SocksResponseStatus> InitiateConnection(IPAddress address, int port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port), "Port number is invalid");

            if (!await _connectedCompletion.Task)
                return SocksResponseStatus.GeneralFailure;

            var request = new Socks5ConnectionRequest(SocksCommand.Connect, address, port);
            var result = await InternalClient.SendSerializable(request);
            if (!result)        // TODO: Error HANDLING
                throw new InvalidOperationException("Something went wrong when sending serialization");

            var connectionResponse = await InternalClient.ReceiveSerializable<Socks5ConnectionResponse>();
            return connectionResponse.Status;
        }

        public Task<SocksResponseStatus> InitiateConnection(IPEndPoint endPoint)
            => InitiateConnection(endPoint.Address, endPoint.Port);

        internal Task<bool> ConnectWith(EndPoint endPoint)
        {
            _endPointClient = new TcpClient();
            return _endPointClient.ConnectAsync(endPoint);
        }

        private async Task<bool> ConnectWithSocksServer(EndPoint endPoint)
        {
            if (!await InternalClient.ConnectAsync(endPoint))
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
        
        internal void StartTunneling()
        {
            _endPointClient.ReceiveCompleted += OnRemoteReceived;
            _endPointClient.ClientDisconnected += async (sender, args) =>
            {
                Console.WriteLine("Disconnected..");
                await Task.Delay(1000).ConfigureAwait(false);
                InternalClient.Disconnect(false);
            };

            InternalClient.ReceiveCompleted += OnProxiedReceived;
            InternalClient.ClientDisconnected += async (sender, args) =>
            {
                Console.WriteLine("Disconnected..");
                await Task.Delay(1000).ConfigureAwait(false);
                _endPointClient.Disconnect(false);
            };

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
