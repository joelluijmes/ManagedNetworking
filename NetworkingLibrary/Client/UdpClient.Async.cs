using NetworkingLibrary.Util;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public sealed partial class UdpClient : IAsyncUdpClient
    {
        private static readonly Pool<SocketAsyncEventArgs> _pool = new Pool<SocketAsyncEventArgs>();

        private delegate bool SocketTransferAsyncFunc(SocketAsyncEventArgs args);
        private delegate Task<Tuple<int, EndPoint>> TransferAsyncFunc(byte[] buffer, int offset, int count, EndPoint endPoint);
        
        public Task<Tuple<int, EndPoint>> SendToAsync(byte[] buffer, int offset, int count, EndPoint endPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return TransferAsync(Socket.SendToAsync, buffer, offset, count, endPoint);
        }

        public Task<bool> SendAllAsync(byte[] buffer, int count, EndPoint endPoint)
        {
            ValidateTransferAllArguments(buffer, ref count);
            return TransferAllAsync(SendToAsync, buffer, count, endPoint);
        }

        public Task<Tuple<int, EndPoint>> ReceiveAsync(byte[] buffer, int offset, int count, EndPoint endPoint)
        {
            ValidateTransferArguments(buffer, offset, ref count);
            return TransferAsync(Socket.ReceiveFromAsync, buffer, offset, count, endPoint);
        }

        public Task<bool> ReceiveAllAsync(byte[] buffer, int count, EndPoint endPoint)
        {
            ValidateTransferAllArguments(buffer, ref count);
            return TransferAllAsync(ReceiveAsync, buffer, count, endPoint);
        }

        private async Task<Tuple<int, EndPoint>> TransferAsync(SocketTransferAsyncFunc func, byte[] buffer, int offset, int count, EndPoint endPoint)
        {
            var tcs = new TaskCompletionSource<bool>();     // use TaskCompletionSource for when the method is running async
            EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
            {
                tcs.SetResult(true);
            };

            var socketArgs = _pool.Get();
            socketArgs.Completed += completedEventHandler;
            socketArgs.SetBuffer(buffer, offset, count);
            socketArgs.RemoteEndPoint = endPoint;

            if (func(socketArgs))                           // running async
                await tcs.Task;                             // so wait for completion

            // ErrorHandling(socketArgs.SocketError);

            socketArgs.SetBuffer(null, 0, 0);
            socketArgs.Completed -= completedEventHandler;
            _pool.Put(socketArgs);

            return Tuple.Create(socketArgs.BytesTransferred, socketArgs.RemoteEndPoint);
        }

        private static async Task<bool> TransferAllAsync(TransferAsyncFunc func, byte[] buffer, int count, EndPoint endPoint)
        {
            var total = 0;
            while (total < count)
            {
                var current = (await func(buffer, total, count - total, endPoint)).Item1;
                if (current == 0)
                    return false;

                total += current;
            }

            return true;
        }
    }
}