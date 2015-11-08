﻿using NetworkingLibrary.Events;
using System;

namespace NetworkingLibrary.Server
{
    public class OverlappedServer : Server
    {
        public event EventHandler<ClientEventArgs> ClientConnected;

        public override T AcceptClient<T>()
        {
            _socket.BeginAccept((result) =>
            {
                var clientSocket = _socket.EndAccept(result);
                var client = CreateClient<T>(clientSocket);

                ClientConnected?.Invoke(this, new ClientEventArgs(client));
            }, null);

            return default(T);
        }
    }
}
