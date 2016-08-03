using NetworkingLibrary.Events;
using System;
using System.Net;

namespace NetworkingLibrary.Client
{
    public interface IEventUdpClient
    {
        event EventHandler<TransferEventArgs> ReceiveCompleted;
        event EventHandler<TransferEventArgs> SendCompleted;

        void BeginReceive(byte[] buffer, int offset, int count, EndPoint remoteEndPoint);
        void BeginReceiveAll(byte[] buffer, int count, EndPoint remoteEndPoint);
        void BeginSend(byte[] buffer, int offset, int count, EndPoint endPoint);
        void BeginSendAll(byte[] buffer, int count, EndPoint remoteEndPoint);
        void Bind(EndPoint endPoint);
    }
}