using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Socks.SOCKS5.Packets;

namespace NetworkingLibrary.Socks.SOCKS5
{
    public class Socks5Client
    {
        private readonly TcpClient _client;

        public Socks5Client()
        {
            _client = new TcpClient();
        }

        public Task<bool> ConnectWithServer(EndPoint serverEndPoint)
            => _client.ConnectAsync(serverEndPoint);

        public async Task<SocksResponseStatus> InitiateConnection(string domain, int port = 80)
        {
            var request = new Socks5ConnectionRequest(SocksCommand.Connect, domain, port);
            var result = await SendSerializable(request);
            if (!result)        // TODO: Error HANDLING
                throw new InvalidOperationException("Something went wrong when sending serialization");

            var connectionResponse = await ReceiveSerializable<Socks5ConnectionResponse>();
            return connectionResponse.Status;
        }

        private async Task<T> ReceiveSerializable<T>() where T : ISerializable, new()
        {
            var serializable = new T();

            var bufHeader = new byte[serializable.HeaderLength];
            if (await _client.ReceiveAllAsync(bufHeader))
                return default(T);

            var bufBody = new byte[serializable.BodyLength];
            return await _client.ReceiveAllAsync(bufBody)
                ? serializable
                : default(T);
        }

        private Task<bool> SendSerializable(ISerializable serializable)
        {
            var buffer = serializable.Serialize();
            return _client.SendAllAsync(buffer);
        }
    }
}
