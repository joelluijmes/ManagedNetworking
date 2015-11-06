using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public class AsyncUdpClient : OverlappedUdpClient, IAsyncUdpClient
    {
        private readonly Pool<SocketAsyncEventArgs> _pool;

        public AsyncUdpClient()
        {
            _pool = new Pool<SocketAsyncEventArgs>();
        }

        public async Task<Tuple<int, EndPoint>> ReceiveFromAsync(byte[] buffer, int offset, int count, EndPoint endPoint)
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

            if (_socket.ReceiveFromAsync(socketArgs))       // running async
                await tcs.Task;                             // so wait for completion

            socketArgs.Completed -= completedEventHandler;

            endPoint = socketArgs.RemoteEndPoint;
            socketArgs.RemoteEndPoint = null;

            var success = socketArgs.SocketError == SocketError.Success;
            _pool.Put(socketArgs);

            return Tuple.Create(success ? socketArgs.BytesTransferred : 0, endPoint);
        }

        public async Task<Tuple<bool, EndPoint>> ReceiveAllFromAsync(byte[] buffer, int count, EndPoint endPoint)
        {
            var totalReceived = 0;
            EndPoint remoteEndPoint = default(EndPoint);

            while (totalReceived < count)
            {
                var tuple = await ReceiveFromAsync(buffer, totalReceived, count - totalReceived, endPoint);
                remoteEndPoint = tuple.Item2;
                if (tuple.Item1 == 0)
                    return Tuple.Create(false, tuple.Item2);
            }

            return Tuple.Create(true, remoteEndPoint);
        }
        
        public async Task<int> SendToAsync(byte[] buffer, int offset, int count, EndPoint endPoint)
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

            if (_socket.SendToAsync(socketArgs))            // running async
                await tcs.Task;                             // so wait for completion

            socketArgs.Completed -= completedEventHandler;
            socketArgs.RemoteEndPoint = null;

            var success = socketArgs.SocketError == SocketError.Success;
            _pool.Put(socketArgs);

            return success ? socketArgs.BytesTransferred : 0;
        }

        public async Task<bool> SendToAllAsync(byte[] buffer, int count, EndPoint endPoint)
        {
            var totalSent = 0;
            while (totalSent < count)
            {
                var sent = await SendToAsync(buffer, totalSent, count - totalSent, endPoint);
                if (sent == 0)
                    return false;

                totalSent += sent;
            }

            return true;
        }
    }
}
