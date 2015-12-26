using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Socks.SOCKS5
{
    public sealed partial class Socks5Client : ITcpClient, IEventTcpClient, IAsyncTcpClient
    {
        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;

        public bool Connect(EndPoint endPoint)
        {	// TODO Replace InitiateConnection with these connects
            throw new NotImplementedException();
        }

        public int Receive(byte[] buffer, int offset = 0, int count = -1)
			=> InternalClient.Receive(buffer, offset, count);

        public bool ReceiveAll(byte[] buffer, int count = -1)
            => InternalClient.ReceiveAll(buffer, count);

        public int Send(byte[] buffer, int offset = 0, int count = -1)
            => InternalClient.Send(buffer, offset, count);

        public bool SendAll(byte[] buffer, int count = -1)
            => InternalClient.SendAll(buffer, count);

        public void BeginConnect(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void BeginSend(byte[] buffer, int offset = 0, int count = -1)
            => InternalClient.BeginSend(buffer, offset, count);

        public void BeginSendAll(byte[] buffer, int count = -1)
            => InternalClient.BeginSendAll(buffer, count);

        public void BeginReceive(byte[] buffer, int offset = 0, int count = -1)
            => InternalClient.BeginReceive(buffer, offset, count);

        public void BeginReceiveAll(byte[] buffer, int count = -1)
            => InternalClient.BeginReceiveAll(buffer, count);

        public Task<bool> ConnectAsync(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReceiveAllAsync(byte[] buffer, int count = -1)
            => InternalClient.ReceiveAllAsync(buffer, count);

        public Task<int> ReceiveAsync(byte[] buffer, int offset = 0, int count = -1)
            => InternalClient.ReceiveAsync(buffer, offset, count);

        public Task<int> SendAsync(byte[] buffer, int offset = 0, int count = -1)
            => InternalClient.SendAsync(buffer, offset, count);

        public Task<bool> SendAllAsync(byte[] buffer, int count = -1)
            => InternalClient.SendAllAsync(buffer, count);
    }
}
