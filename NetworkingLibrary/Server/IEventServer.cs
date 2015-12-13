using System;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Server
{
    public interface IEventServer
    {
        event EventHandler<ClientEventArgs> ClientConnected;

        void BeginAccept();
    }
}
