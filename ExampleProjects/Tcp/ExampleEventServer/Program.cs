using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Client;
using NetworkingLibrary.Events;
using NetworkingLibrary.Server;

namespace ExampleEventServer
{
    class Program
    {
        private static TcpServer server;
        static void Main(string[] args)
        {
            server = new TcpServer();
            server.ClientConnected += ServerOnClientConnected;

            server.Listen(1111);
            server.BeginAccept();

            Console.WriteLine("Accepting clients..");
            Console.ReadLine();
        }

        private static void ServerOnClientConnected(object sender, ClientEventArgs clientEventArgs)
        {
            Console.WriteLine("Client connected!");

            var client = clientEventArgs.Client;
            client.ReceiveCompleted += ClientOnReceiveCompleted;
            client.BeginReceive(new byte[2048]);

            server.BeginAccept();
        }

        private static void ClientOnReceiveCompleted(object sender, TransferEventArgs transferEventArgs)
        {
            if (transferEventArgs.Count == 0)
            {
                Console.WriteLine($"[{transferEventArgs.Client.RemoteEndPoint}]: Disconnected");
                return;
            }

            var str = Encoding.UTF8.GetString(transferEventArgs.Bytes, 0, transferEventArgs.Count);
            Console.WriteLine($"[{transferEventArgs.Client.RemoteEndPoint}]: {str}");
            (transferEventArgs.Client as TcpClient)?.BeginReceive(new byte[2048]);
        }
    }
}
