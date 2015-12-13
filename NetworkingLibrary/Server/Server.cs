using System.Net;
using System.Net.Sockets;
using TcpClient = NetworkingLibrary.Client.TcpClient;

namespace NetworkingLibrary.Server
{
    public partial class Server : IServer
    {
        private const int BACKLOG = 200;
        private Socket _socket;

        public Server()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Listen(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socket.Bind(endPoint);
            _socket.Listen(BACKLOG);
        }

        public void Stop()
        {
            _socket.Close();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        
        public TcpClient AcceptClient()
        {
            var clientSocket = _socket.Accept();
            return new TcpClient(clientSocket);
        }
    }
}
