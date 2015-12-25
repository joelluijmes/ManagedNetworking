using System;
using NetworkingLibrary.Client;

namespace NetworkingLibrary.Events
{
    public class ClientEventArgs : EventArgs
    {
        public TcpClient Client { get; }

        public ClientEventArgs(TcpClient client)
        {
            Client = client;
        }
    }
}
