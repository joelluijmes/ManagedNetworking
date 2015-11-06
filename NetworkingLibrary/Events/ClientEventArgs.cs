using System;
using NetworkingLibrary.Client;

namespace NetworkingLibrary.Events
{
    public class ClientEventArgs : EventArgs
    {
        public ITcpClient Client { get; }

        public ClientEventArgs(ITcpClient client)
        {
            Client = client;
        }
    }
}
