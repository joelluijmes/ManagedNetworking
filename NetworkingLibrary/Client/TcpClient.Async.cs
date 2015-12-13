using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkingLibrary.Util;

namespace NetworkingLibrary.Client
{
    public sealed partial class TcpClient : IAsyncTcpClient
    {
        private readonly Pool<SocketAsyncEventArgs> _pool = new Pool<SocketAsyncEventArgs>();
		
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

        public Task<bool> SendAllAsync(byte[] buffer, int count)
            => TransceiveAllAsync(SendAsync, buffer, count);

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

        public Task<bool> ReceiveAllAsync(byte[] buffer, int count)
            => TransceiveAllAsync(ReceiveAsync, buffer, count);

        private static async Task<bool> TransceiveAllAsync(Func<byte[], int, int, Task<int>> func, byte[] buffer, int count)
        {
            var total = 0;
            while (total < count)
            {
                var current = await func(buffer, total, count - total);
                if (current == 0)
                    return false;

                total += current;
            }

            return true;
        }
    }
}
