using NetworkingLibrary.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public class OverlappedClient : Client
    {
        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;

        public OverlappedClient() { }
        public OverlappedClient(ProtocolType type) : base(type) { }
        internal OverlappedClient(Socket socket) : base(socket) { }

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

        public override int SendTo(byte[] buffer, int offset, int count, EndPoint endPoint)
        {
            _socket.BeginSendTo(buffer, offset, count, SocketFlags.None, endPoint, (result) =>
            {
                var sent = _socket.EndSendTo(result);

                SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, sent, endPoint));
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

        public override bool SendToAll(byte[] buffer, int count, EndPoint endPoint)
        {
            var totalSent = 0;
            AsyncCallback sendCallback = null;

            sendCallback = (result) =>
            {
                var sent = _socket.EndSendTo(result);
                totalSent += sent;

                if (totalSent < count)
                    _socket.BeginSendTo(buffer, totalSent, count - totalSent, SocketFlags.None, endPoint, sendCallback, null);
                else
                    SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, totalSent, endPoint));
            };

            _socket.BeginSendTo(buffer, totalSent, count - totalSent, SocketFlags.None, endPoint, sendCallback, null);
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

        public override int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint endPoint)
        {
            _socket.BeginReceiveFrom(buffer, offset, count, SocketFlags.None, ref endPoint, (result) =>
            {
                var end = (EndPoint)result.AsyncState;
                var received = _socket.EndReceiveFrom(result, ref end);

                ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, received, end));
            }, endPoint);

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

        public override bool ReceiveFromAll(byte[] buffer, int count, ref EndPoint endPoint)
        {
            var totalReceived = 0;
            AsyncCallback receiveCallback = null;

            receiveCallback = (result) =>
            {
                var end = (EndPoint)result.AsyncState;
                var received = _socket.EndReceiveFrom(result, ref end);
                totalReceived += received;

                if (totalReceived < count)
                    _socket.BeginReceiveFrom(buffer, totalReceived, count - totalReceived, SocketFlags.None, ref end, receiveCallback, null);
                else
                    ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, totalReceived, end));
            };

            _socket.BeginReceiveFrom(buffer, totalReceived, count - totalReceived, SocketFlags.None, ref endPoint, receiveCallback, null);
            return true;
        }
    }
}
