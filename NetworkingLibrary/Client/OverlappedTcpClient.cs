using NetworkingLibrary.Events;
using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public class OverlappedTcpClient : TcpClient, IOverlappedClient
    {
        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;
        
        public override bool Connect(EndPoint endPoint)
        {
            try
            {
                _socket.BeginConnect(endPoint, (result) =>
                {
                    _socket.EndConnect(result);
                    ClientConnected?.Invoke(this, new ClientEventArgs(this));
                }, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public override int Send(byte[] buffer, int offset, int count)
        {
            _socket.BeginSend(buffer, offset, count, SocketFlags.None, (result) =>
            {
                var sent = _socket.EndSend(result);

                SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, sent));
            }, null);

            return 0;
        }
        
        public override bool SendAll(byte[] buffer, int count)
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
            return true;
        }

        public override int Receive(byte[] buffer, int offset, int count)
        {
            _socket.BeginReceive(buffer, offset, count, SocketFlags.None, (result) =>
            {
                var received = _socket.EndReceive(result);
                
                ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, received));
            }, null);

            return 0;
        }
        
        public override bool ReceiveAll(byte[] buffer, int count)
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

            return true;
        }
    }
}
