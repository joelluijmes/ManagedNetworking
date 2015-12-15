using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkingLibrary.Util;

namespace NetworkingLibrary.Client
{
	public sealed partial class TcpClient : IAsyncTcpClient
	{
		private static readonly Pool<SocketAsyncEventArgs> _pool = new Pool<SocketAsyncEventArgs>();

		private delegate bool SocketTransferAsyncFunc(SocketAsyncEventArgs args);

		private delegate Task<int> TransferAsyncFunc(byte[] buffer, int offset, int count);
		
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

			if (Socket.ConnectAsync(socketArgs))           // running async
				await tcs.Task;                             // so wait for completion

			socketArgs.Completed -= completedEventHandler;

			var success = socketArgs.SocketError == SocketError.Success;
			_pool.Put(socketArgs);

			return success;
		}

		public Task<int> SendAsync(byte[] buffer, int offset = 0, int count = -1)
		{
            ValidateTransferArguments(buffer, offset, ref count);
            return TransferAsync(Socket.SendAsync, buffer, offset, count);
		}

	    public Task<bool> SendAllAsync(byte[] buffer, int count = -1)
	    {
            ValidateTransferAllArguments(buffer, ref count);
            return TransferAllAsync(SendAsync, buffer, count);
	    }

	    public Task<int> ReceiveAsync(byte[] buffer, int offset = 0, int count = -1)
	    {
            ValidateTransferArguments(buffer, offset, ref count);
            return TransferAsync(Socket.ReceiveAsync, buffer, offset, count);
	    }

	    public Task<bool> ReceiveAllAsync(byte[] buffer, int count = -1)
	    {
            ValidateTransferAllArguments(buffer, ref count);
            return TransferAllAsync(ReceiveAsync, buffer, count);
	    }

	    private static async Task<int> TransferAsync(SocketTransferAsyncFunc func, byte[] buffer, int offset, int count)
		{
            var tcs = new TaskCompletionSource<bool>();     // use TaskCompletionSource for when the method is running async
			EventHandler<SocketAsyncEventArgs> completedEventHandler = (sender, e) =>
			{
				tcs.SetResult(true);
			};

			var socketArgs = _pool.Get();
			socketArgs.Completed += completedEventHandler;
			socketArgs.SetBuffer(buffer, offset, count);

			if (func(socketArgs))                           // running async
				await tcs.Task;                             // so wait for completion

			socketArgs.Completed -= completedEventHandler;

			// TODO: More error handling
			var success = socketArgs.SocketError == SocketError.Success;
			_pool.Put(socketArgs);

			return success ? socketArgs.BytesTransferred : 0;
		}

		private static async Task<bool> TransferAllAsync(TransferAsyncFunc func, byte[] buffer, int count)
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
