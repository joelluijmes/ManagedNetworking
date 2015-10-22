using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary
{
    public class Server
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
            _socket.Bind(endPoint);
            _socket.Listen(BACKLOG);
        }

        public virtual T AcceptClient<T>() where T : IClient
        {
            var socket = _socket.Accept();

            var creator = Creator<T>.GetCreator();
            return creator(socket);
        }
    }
}
