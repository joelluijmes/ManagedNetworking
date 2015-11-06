using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public sealed class AsyncClient : OverlappedClient, IAsyncClient
    {
        private readonly Pool<SocketAsyncEventArgs> _pool;

        public AsyncClient()
        {
            _pool = new Pool<SocketAsyncEventArgs>();
        }

        internal AsyncClient(Socket socket) : base(socket)
        {
            _pool = new Pool<SocketAsyncEventArgs>();
        }

        public async Task<bool> ConnectAsync(EndPoint endPoint)
        {
            var tcs = new TaskCompletionSource<bool>();     // use TaskCompletionSource for when the method is running async
            EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
            };

            var socketArgs = _pool.Get();
            socketArgs.Completed += completedEventHandler;
            socketArgs.RemoteEndPoint = endPoint;

            if (_socket.ConnectAsync(socketArgs))           // running async
                await tcs.Task;                             // so wait for completion

            socketArgs.Completed -= completedEventHandler;

            var success = socketArgs.SocketError == SocketError.Success;
            _pool.Put(socketArgs);

            return success;
        }

        public async Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<bool>();     // use TaskCompletionSource for when the method is running async
            EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
            };

            var socketArgs = _pool.Get();
            socketArgs.Completed += completedEventHandler;
            socketArgs.SetBuffer(buffer, offset, count);
          
            if (_socket.SendAsync(socketArgs))              // running async
                await tcs.Task;                             // so wait for completion

            socketArgs.Completed -= completedEventHandler;

            var success = socketArgs.SocketError == SocketError.Success;
            _pool.Put(socketArgs);

            return success ? socketArgs.BytesTransferred : 0;
        }

        public async Task<bool> SendAllAsync(byte[] buffer, int count)
        {
            var totalSent = 0;
            while (totalSent < count)
            {
                var sent = await SendAsync(buffer, totalSent, count - totalSent);
                if (sent == 0)
                    return false;

                totalSent += sent;
            }

            return true;
        }

        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<bool>();     // use TaskCompletionSource for when the method is running async
            EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
            };

            var socketArgs = _pool.Get();
            socketArgs.Completed += completedEventHandler;
            socketArgs.SetBuffer(buffer, offset, count);

            if (_socket.ReceiveAsync(socketArgs))           // running async
                await tcs.Task;                             // so wait for completion

            socketArgs.Completed -= completedEventHandler;

            var success = socketArgs.SocketError == SocketError.Success;
            _pool.Put(socketArgs);

            return success ? socketArgs.BytesTransferred : 0;
        }

        public async Task<bool> ReceiveAllAsync(byte[] buffer, int count)
        {
            var totalReceived = 0;
            while (totalReceived < count)
            {
                var received = await ReceiveAsync(buffer, totalReceived, count - totalReceived);
                if (received == 0)
                    return false;
            }

            return true;
        }
    }
}
