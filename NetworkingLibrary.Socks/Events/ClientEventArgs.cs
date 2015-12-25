using System;
using NetworkingLibrary.Socks.SOCKS5;

namespace NetworkingLibrary.Socks.Events
{
    public sealed class ClientEventArgs : EventArgs
    {
        public Socks5Client Client { get; }

        public ClientEventArgs(Socks5Client client)
        {
            Client = client;
        }
    }
}
