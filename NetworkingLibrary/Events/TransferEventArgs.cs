using System;
using System.Net;
using NetworkingLibrary.Client;

namespace NetworkingLibrary.Events
{
    public class TransferEventArgs : EventArgs
    {
        public BaseClient Client { get; }
        public byte[] Bytes { get; }
        public int Count { get; }
        public EndPoint EndPoint { get; }

        public TransferEventArgs(TcpClient client, byte[] bytes, int count)
        {
            Client = client;
            Bytes = bytes;
            Count = count;
        }

        public TransferEventArgs(UdpClient client, byte[] bytes, int count, EndPoint endPoint)
        {
            Client = client;
            Bytes = bytes;
            Count = count;
            EndPoint = endPoint;
        }
    }
}
