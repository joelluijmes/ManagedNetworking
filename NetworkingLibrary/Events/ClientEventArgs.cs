using System;

namespace NetworkingLibrary.Events
{
    public class ClientEventArgs : EventArgs
    {
        public IClient Client { get; }

        public ClientEventArgs(IClient client)
        {
            Client = client;
        }
    }
}
