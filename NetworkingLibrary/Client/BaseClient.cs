using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NetworkingLibrary.Client
{
    public abstract class BaseClient : IDisposable
    {
        private readonly ProtocolType _type;
        private Socket _socket;

        protected Socket Socket
        {
            get { return _socket; }
            set
            {
                _socket = value;
                NetworkStream?.Dispose();

                if (_socket?.Connected == true)
                    NetworkStream = new NetworkStream(_socket, false);
            }
        }
        
        public EndPoint RemoteEndPoint => Socket?.RemoteEndPoint;
        public EndPoint LocalEndPoint => Socket?.LocalEndPoint;

        public NetworkStream NetworkStream { get; private set; }
          
        protected BaseClient(ProtocolType type)
        {
            _type = type;
            Socket = CreateSocket(type);
        }

        internal BaseClient(Socket socket)
        {
            _type = socket.ProtocolType;
            Socket = socket;
        }

        public void Dispose()
        {
            if (Socket != null)
            {
                if (Socket.Connected)
                    Socket.Shutdown(SocketShutdown.Both);

                Socket.Close();
                Socket.Dispose();
                Socket = null;
            }

            if (NetworkStream != null)
            {
                NetworkStream.Dispose();
                NetworkStream = null;
            }
        }

        protected void CreateSocket()
        {
            if (Socket != null)
                throw new InvalidOperationException("There is already a socket! Disconnect first before creating a new one.");

            Socket = CreateSocket(_type);
        }

        private static Socket CreateSocket(ProtocolType type)
        {
            switch (type)
            {
                case ProtocolType.Udp:
                    return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, type);
                case ProtocolType.Tcp:
                    return new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
                default:
                    throw new NotImplementedException("Unable to instantiate non tcp/udp client.");
            }
        }

        protected static void ValidateTransferArguments(byte[] buffer, int offset, ref int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (count == -1)
                count = buffer.Length;
            if (offset < 0)
                throw new ArgumentException("Offset must be positive.", nameof(offset));
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));
        }

        protected static void ValidateTransferAllArguments(byte[] buffer, ref int count)
        {
            ValidateTransferArguments(buffer, 0, ref count);
        }
    }
}
