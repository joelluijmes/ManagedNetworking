using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Server
{
    public partial class Server : IEventServer
    {
        public event EventHandler<ClientEventArgs> ClientConnected;

        public void BeginAccept()
        {
            throw new NotImplementedException();
        }
    }
}
