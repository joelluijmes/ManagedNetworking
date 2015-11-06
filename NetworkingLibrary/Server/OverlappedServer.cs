using NetworkingLibrary.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Server
{
    public class OverlappedServer : Server
    {
        public event EventHandler<ClientEventArgs> ClientConnected;

        public override T AcceptClient<T>()
        {
            _socket.BeginAccept((result) =>
            {
                var clientSocket = _socket.EndAccept(result);

                var creator = Creator<T>.GetCreator();
                var client = creator(clientSocket);
                ClientConnected?.Invoke(this, new ClientEventArgs(client));
            }, null);

            return default(T);
        }
    }
}
