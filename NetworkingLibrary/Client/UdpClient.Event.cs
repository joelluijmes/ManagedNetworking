using System;
using System.Net;
using System.Net.Sockets;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Client
{
    public sealed partial class UdpClient
    {
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;

        public void BeginSend(byte[] buffer, int offset, int count, EndPoint endPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            Socket.BeginSend(buffer, offset, count, SocketFlags.None, ar =>
            {
                var sent = Socket.EndSend(ar);

                SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, sent, endPoint));
            }, null);
        }

        public void BeginSendAll(byte[] buffer, int count, EndPoint remoteEndPoint)
        {
            ValidateTransferAllArguments(buffer, ref count);

            var transfered = 0;
            AsyncCallback callback = null;
            callback = result =>
            {
                var current = Socket.EndSendTo(result);
                if (current == 0)
                {
                    SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, transfered, remoteEndPoint));
                    return;
                }

                transfered += current;
                if (transfered < current)
                    Socket.BeginSendTo(buffer, transfered, count - transfered, SocketFlags.None, remoteEndPoint, callback, null);
                else
                    SendCompleted?.Invoke(this, new TransferEventArgs(this, buffer, transfered, remoteEndPoint));
            };

            Socket.BeginSendTo(buffer, transfered, count - transfered, SocketFlags.None, remoteEndPoint, callback, null);
        }

        public void BeginReceive(byte[] buffer, int offset, int count, EndPoint remoteEndPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            Socket.BeginReceiveFrom(buffer, offset, count, SocketFlags.None, ref remoteEndPoint, ar =>
            {
                var remoteEnd = (EndPoint)ar.AsyncState;
                var received = Socket.EndReceiveFrom(ar, ref remoteEnd);

                ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, received, remoteEnd));
            }, remoteEndPoint);
        }

        public void BeginReceiveAll(byte[] buffer, int count, EndPoint remoteEndPoint)
        {
            ValidateTransferAllArguments(buffer, ref count);

            var transfered = 0;
            AsyncCallback callback = null;
            callback = result =>
            {
                var endPoint = result.AsyncState as EndPoint;
                if (endPoint == null)
                    return;

                var current = Socket.EndReceiveFrom(result, ref endPoint);
                if (current == 0)
                {
                    ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, transfered, endPoint));
                    return;
                }

                transfered += current;
                if (transfered < current)
                    Socket.BeginReceiveFrom(buffer, transfered, count - transfered, SocketFlags.None, ref endPoint, callback, endPoint);
                else
                    ReceiveCompleted?.Invoke(this, new TransferEventArgs(this, buffer, transfered, endPoint));
            };

            Socket.BeginReceiveFrom(buffer, transfered, count - transfered, SocketFlags.None, ref remoteEndPoint, callback, remoteEndPoint);
        }
    }
}