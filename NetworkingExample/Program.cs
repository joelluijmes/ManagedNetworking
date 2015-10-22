using NetworkingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Events;
using System.Reflection;
using System.Globalization;
using System.Net.Sockets;

namespace NetworkingExample
{
    class Program
    {
        static async void ServerTask()
        {
            var server = new AsyncServer();
            server.Listen(2222);
            server.ClientConnected += (o, e) => Console.WriteLine("Client connected");

            while (true)
            {
                var client = await server.AcceptClientAsync<AsyncClient>();
                Console.WriteLine("Client connectedd");
            }
        }
        
        static void Main(string[] args)
        {
            Task.Run(() => ServerTask());
                        
            Task.Run(() =>
            {
                var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);

                var client = new Client();
                client.Connect(endPoint);
            });

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
