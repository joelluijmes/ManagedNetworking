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

        private delegate IAsyncResult BeginSocketTransferFunc(byte[] buffer, int offset, int count, SocketFlags socketFlags, AsyncCallback callback, object state);
        private delegate int EndSocketTransferFunc(IAsyncResult result);
        
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
            BeginTransfer(_socket.BeginSend, _socket.EndSend, buffer, offset, count, SendCompleted);
        }

        public void BeginSendAll(byte[] buffer, int count)
        {
            BeginTransferAll(_socket.BeginSend, _socket.EndSend, buffer, count, SendCompleted);
        }

        public void BeginReceive(byte[] buffer, int offset, int count)
        {
            BeginTransfer(_socket.BeginReceive, _socket.EndReceive, buffer, offset, count, ReceiveCompleted);
        }

        public void BeginReceiveAll(byte[] buffer, int count)
        {
            BeginTransferAll(_socket.BeginReceive, _socket.EndReceive, buffer, count, ReceiveCompleted);
        }

        private void BeginTransfer(BeginSocketTransferFunc beginFunc, EndSocketTransferFunc endFunc, byte[] buffer, int offset, int count, EventHandler<TransferEventArgs> completedHandler)
        {
            beginFunc(buffer, offset, count, SocketFlags.None, result =>
            {
                var transfered = endFunc(result);
                // TODO: Error handling

                completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, transfered));
            }, null);
        }

        private void BeginTransferAll(BeginSocketTransferFunc beginFunc, EndSocketTransferFunc endFunc, byte[] buffer, int count, EventHandler<TransferEventArgs> completedHandler)
        {
            var transfered = 0;
            AsyncCallback callback = null;

            callback = result =>
            {
                var current = endFunc(result);
                transfered += current;

                if (transfered < count)
                    beginFunc(buffer, transfered, count - transfered, SocketFlags.None, callback, null);
                else
                    completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, count));

                // TODO: Error handling
                completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, transfered));
            };

            beginFunc(buffer, transfered, count - transfered, SocketFlags.None, callback, null);
        }
    }
}
