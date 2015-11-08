using NetworkingLibrary.Client;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Util;

namespace NetworkingLibrary.Server
{
    public class AsyncServer : OverlappedServer, IAsyncServer
    {
        private readonly Pool<SocketAsyncEventArgs> _pool;

        public AsyncServer()
        {
            _pool = new Pool<SocketAsyncEventArgs>();
        }

        internal AsyncServer(Socket socket)
        {
            _pool = new Pool<SocketAsyncEventArgs>();
        }

        public async Task<T> AcceptClientAsync<T>() where T : BaseClient, ITcpClient
        {
            var tcs = new TaskCompletionSource<T>();     // use TaskCompletionSource for when the method is running async
            EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
            {
                var client = CreateClient<T>(e.AcceptSocket);

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
