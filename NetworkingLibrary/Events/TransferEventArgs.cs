using System;
using System.Net;
using NetworkingLibrary.Client;

namespace NetworkingLibrary.Events
{
    public class TransferEventArgs : EventArgs
    {
        public IEventTcpClient Client { get; }
        public byte[] Bytes { get; }
        public int Count { get; }
        public EndPoint EndPoint { get; }

        public TransferEventArgs(IEventTcpClient client, byte[] bytes, int count)
        {
            Client = client;
            Bytes = bytes;
            Count = count;
        }

        public TransferEventArgs(IEventTcpClient client, byte[] bytes, int count, EndPoint endPoint) : this(client, bytes, count)
        {
            EndPoint = endPoint;
        }
    }
}
