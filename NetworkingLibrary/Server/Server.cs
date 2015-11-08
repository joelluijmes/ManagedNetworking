using NetworkingLibrary.Client;
using System.Net;
using System.Net.Sockets;
using Util;
using System;

namespace NetworkingLibrary.Server
{
    public class Server : BaseServer
    {
        private const int BACKLOG = 200;
        protected Socket _socket;

        public Server()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Listen(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socket.Bind(endPoint);
            _socket.Listen(BACKLOG);
        }

        public void Stop()
        {
            _socket.Close();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        protected override Socket AcceptClient()
            => _socket.Accept();
    }
}
