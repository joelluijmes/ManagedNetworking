using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public event EventHandler<ClientEventArgs> ClientDisconnected; 
        public event EventHandler<TransferEventArgs> SendCompleted;
        public event EventHandler<TransferEventArgs> ReceiveCompleted;

        private delegate IAsyncResult BeginSocketTransferFunc(byte[] buffer, int offset, int count, SocketFlags socketFlags, AsyncCallback callback, object state);
        private delegate int EndSocketTransferFunc(IAsyncResult result, out SocketError socketError);
        
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
            ValidateTransferArguments(buffer, offset, ref count);
            if (Socket.Connected)
                BeginTransfer(Socket.BeginSend, Socket.EndSend, buffer, offset, count, SendCompleted);
        }

        public void BeginSendAll(byte[] buffer, int count = -1)
        {
            ValidateTransferAllArguments(buffer, ref count);
            if (Socket.Connected)
                BeginTransferAll(Socket.BeginSend, Socket.EndSend, buffer, count, SendCompleted);
        }

        public void BeginReceive(byte[] buffer, int offset = 0, int count = -1)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            // if (Socket.Connected)
                BeginTransfer(Socket.BeginReceive, Socket.EndReceive, buffer, offset, count, ReceiveCompleted);
        }

        public void BeginReceiveAll(byte[] buffer, int count = -1)
        {
            ValidateTransferAllArguments(buffer, ref count);
            if (Socket.Connected)
                BeginTransferAll(Socket.BeginReceive, Socket.EndReceive, buffer, count, ReceiveCompleted);
        }

        private void BeginTransfer(BeginSocketTransferFunc beginFunc, EndSocketTransferFunc endFunc, byte[] buffer, int offset, int count, EventHandler<TransferEventArgs> completedHandler)
        {
            ValidateTransferArguments(buffer, offset, ref count);

            try
            {
                beginFunc(buffer, offset, count, SocketFlags.None, result =>
                {
                    try
                    {
                        SocketError error;
                        var transfered = endFunc(result, out error);

                        ErrorHandling(error);
                        completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, transfered));
                    }
                    catch
                    {
                        Disconnect();
                    }
                }, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private void BeginTransferAll(BeginSocketTransferFunc beginFunc, EndSocketTransferFunc endFunc, byte[] buffer, int count, EventHandler<TransferEventArgs> completedHandler)
        {
            var transfered = 0;
            AsyncCallback callback = null;

            callback = result =>
            {
                SocketError error;
                var current = endFunc(result, out error);
                ErrorHandling(error);

                if (current == 0)
                {
                    completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, transfered));
                    Disconnect(false);
                    return;
                }

                transfered += current;
                if (transfered < count)
                    beginFunc(buffer, transfered, count - transfered, SocketFlags.None, callback, null);
                else
                    completedHandler?.Invoke(this, new TransferEventArgs(this, buffer, count));
            };

            try
            {
                beginFunc(buffer, transfered, count, SocketFlags.None, callback, null);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.ConnectionAborted)
                    throw e;

                Debug.WriteLine("ConnectionAborted.. :(");
                Disconnect();
            }
        }
    }
}
