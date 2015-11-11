using System;
using System.Net;
using System.Net.Sockets;
using Util;

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
            if (type == ProtocolType.Udp)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, type);
            else if (type == ProtocolType.Tcp)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
            else
                throw new NotImplementedException("Unable to instantiate non tcp/udp client.");
        }

        internal static T CreateClient<T>(Socket socket) where T : BaseClient
        {
            var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            var client = Creator<T>.GetCreator(constructor).Invoke(null);

            client._socket = socket;
            if (client._socket.Connected)
                client._stream = new NetworkStream(client._socket, false);

            return client;
        }

        public T ConvertTo<T>() where T : BaseClient
            => CreateClient<T>(_socket);
    }
}
