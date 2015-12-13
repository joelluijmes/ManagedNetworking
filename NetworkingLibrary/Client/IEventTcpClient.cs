using System;
using System.Net;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Client
{
    public interface IEventTcpClient
    {
        event EventHandler<ClientEventArgs> ClientConnected;
        event EventHandler<TransferEventArgs> SendCompleted;
        event EventHandler<TransferEventArgs> ReceiveCompleted;

        void BeginConnect(EndPoint endPoint);
        void BeginSend(byte[] buffer, int offset, int count);
        void BeginSendAll(byte[] buffer, int count);
        void BeginReceive(byte[] buffer, int offset, int count);
        void BeginReceiveAll(byte[] buffer, int count);
    }
}