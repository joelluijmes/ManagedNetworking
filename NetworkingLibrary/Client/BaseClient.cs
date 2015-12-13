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

        private NetworkStream _stream;
        public NetworkStream NetworkStream
            => _stream ?? (_stream = (_socket?.Connected == true ? new NetworkStream(_socket, false) : null));
          
        protected BaseClient(ProtocolType type)
        {
            switch (type)
            {
                case ProtocolType.Udp:
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, type);
                    break;
                case ProtocolType.Tcp:
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
                    break;
                default:
                    throw new NotImplementedException("Unable to instantiate non tcp/udp client.");
            }
        }

        internal BaseClient(Socket socket)
        {
            _socket = socket;
        }
    }
}
