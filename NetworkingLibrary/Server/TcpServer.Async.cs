using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkingLibrary.Util;
using TcpClient = NetworkingLibrary.Client.TcpClient;

namespace NetworkingLibrary.Server
{
    public partial class Server : IAsyncServer
    {
        private readonly Pool<SocketAsyncEventArgs> _pool = new Pool<SocketAsyncEventArgs>();

        public async Task<TcpClient> AcceptClientAsync()
        {
            var tcs = new TaskCompletionSource<TcpClient>();     // use TaskCompletionSource for when the method is running async
            EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
            {
                var client = new TcpClient(e.AcceptSocket);

                tcs.SetResult(client);
            };

            var socketArgs = _pool.Get();
            socketArgs.Completed += completedEventHandler;

            if (_socket.AcceptAsync(socketArgs))        // running async
                await tcs.Task;                         // so wait for completion
            else
                completedEventHandler(this, socketArgs);

            socketArgs.AcceptSocket = null;
            socketArgs.Completed -= completedEventHandler;
            _pool.Put(socketArgs);

            return tcs.Task.Result;
        }
    }
}
