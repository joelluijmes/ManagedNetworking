using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;

namespace NetworkingLibrary.Server
{
    public partial class Server : IAsyncServer
    {
        public async Task<TcpClient> AcceptClientAsync()
        {
            throw new NotImplementedException();
        }
    }
}
