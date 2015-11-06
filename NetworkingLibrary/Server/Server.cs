﻿using NetworkingLibrary.Client;
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

namespace NetworkingLibrary.Server
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
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socket.Bind(endPoint);
            _socket.Listen(BACKLOG);
        }

        public void Stop()
        {
            _socket.Close();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual T AcceptClient<T>() where T : IClient
        {
            var socket = _socket.Accept();

            var creator = Creator<T>.GetCreator();
            return creator(socket);
        }
    }
}
