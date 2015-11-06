using NetworkingLibrary.Events;
using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public class OverlappedUdpClient : UdpClient, IOverlappedClient
    {
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;

        public override int SendTo(byte[] buffer, int offset, int count, EndPoint endPoint)
        {
            _socket.BeginSendTo(buffer, offset, count, SocketFlags.None, endPoint, (result) =>
            {
                var sent = _socket.EndSendTo(result);

                SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, sent, endPoint));
            }, null);

            return 0;
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
