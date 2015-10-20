using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Events
{
    public class ClientEventArgs : EventArgs
    {
        public OverlappedClient Client { get; }

        public ClientEventArgs(OverlappedClient client)
        {
            Client = client;
        }
    }
}
