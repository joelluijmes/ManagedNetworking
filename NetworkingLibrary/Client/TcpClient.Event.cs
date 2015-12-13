using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Client
{
    public sealed partial class TcpClient : IEventTcpClient
    {
        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;

        public void BeginConnect(EndPoint endPoint)
        {
            try
            {
                _socket.BeginConnect(endPoint, (result) =>
                {
                    _socket.EndConnect(result);
                    ClientConnected?.Invoke(this, new ClientEventArgs(this));
                }, null);
            }
            catch
            {
                // TODO: Add error handling
            }
        }

        public void BeginSend(byte[] buffer, int offset, int count)
        {
            _socket.BeginSend(buffer, offset, count, SocketFlags.None, (result) =>
            {
                var sent = _socket.EndSend(result);

                SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, sent));
            }, null);
        }

        public void BeginSendAll(byte[] buffer, int count)
        {
            var totalSent = 0;
            AsyncCallback sendCallback = null;

            sendCallback = (result) =>
            {
                var sent = _socket.EndSend(result);
                totalSent += sent;

                if (totalSent < count)
                    _socket.BeginSend(buffer, totalSent, count - totalSent, SocketFlags.None, sendCallback, null);
                else
                    SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, totalSent));
            };

            _socket.BeginSend(buffer, totalSent, count - totalSent, SocketFlags.None, sendCallback, null);
        }

        public void BeginReceive(byte[] buffer, int offset, int count)
        {
            _socket.BeginReceive(buffer, offset, count, SocketFlags.None, (result) =>
            {
                var received = _socket.EndReceive(result);

                ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, received));
            }, null);
        }

        public void BeginReceiveAll(byte[] buffer, int count)
        {
            var totalReceived = 0;
            AsyncCallback receiveCallback = null;

            receiveCallback = (result) =>
            {
                var received = _socket.EndReceive(result);
                totalReceived += received;

                if (totalReceived < count)
                    _socket.BeginReceive(buffer, totalReceived, count - totalReceived, SocketFlags.None, receiveCallback, null);
                else
                    ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, totalReceived));
            };
        }
    }
}
