using System;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Client
{
    public interface IOverlappedClient
    {
        event EventHandler<TransferEventArgs> ReceiveCompleted;
        event EventHandler<TransferEventArgs> SendCompleted;
    }
}