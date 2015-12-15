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
                Socket.BeginConnect(endPoint, (result) =>
                {
                    Socket.EndConnect(result);
                    ClientConnected?.Invoke(this, new ClientEventArgs(this));
                }, null);
            }
            catch
            {
                // TODO: Add error handling
            }
        }

        public void BeginSend(byte[] buffer, int offset = 0, int count = -1)
        {
            BeginTransfer(Socket.BeginSend, Socket.EndSend, buffer, offset, count, SendCompleted);
        }

        public void BeginSendAll(byte[] buffer, int count = -1)
        {
            BeginTransferAll(Socket.BeginSend, Socket.EndSend, buffer, count, SendCompleted);
        }

        public void BeginReceive(byte[] buffer, int offset = 0, int count = -1)
        {
            BeginTransfer(Socket.BeginReceive, Socket.EndReceive, buffer, offset, count, ReceiveCompleted);
        }

        public void BeginReceiveAll(byte[] buffer, int count = -1)
        {
            BeginTransferAll(Socket.BeginReceive, Socket.EndReceive, buffer, count, ReceiveCompleted);
        }

        private void BeginTransfer(BeginSocketTransferFunc beginFunc, EndSocketTransferFunc endFunc, byte[] buffer, int offset, int count, EventHandler<TransferEventArgs> completedHandler)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (offset < 0)
                throw new ArgumentException("Offset must be positive.", nameof(offset));
            if (count == -1)
                count = buffer.Length;
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            beginFunc(buffer, offset, count, SocketFlags.None, result =>
            {
                var transfered = endFunc(result);
                // TODO: Error handling

                completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, transfered));
            }, null);
        }

        private void BeginTransferAll(BeginSocketTransferFunc beginFunc, EndSocketTransferFunc endFunc, byte[] buffer, int count, EventHandler<TransferEventArgs> completedHandler)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (count == -1)
                count = buffer.Length;
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

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
