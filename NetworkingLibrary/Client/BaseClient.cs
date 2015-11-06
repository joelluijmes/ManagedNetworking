using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLibrary.Client
{
    public abstract class BaseClient
    {
        protected Socket _socket;

        public EndPoint RemoteEndPoint => _socket?.RemoteEndPoint;
        public EndPoint LocalEndPoint => _socket?.LocalEndPoint;

        protected BaseClient(ProtocolType type)
        {
            if (type == ProtocolType.Udp)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, type);
            else if (type == ProtocolType.Tcp)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
            else
                throw new NotImplementedException("Unable to instantiate non tcp/udp client.");
        }

        internal BaseClient(Socket socket)
        {
            _socket = socket;
        }
    }
}
