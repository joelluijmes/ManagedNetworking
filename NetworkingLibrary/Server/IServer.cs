using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;

namespace NetworkingLibrary.Server
{
    public interface IServer
    {
        TcpClient AcceptClient();
    }
}
