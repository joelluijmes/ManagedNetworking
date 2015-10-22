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
        static void ServerTask()
        {
            var server = new OverlappedServer();
            server.Listen(2222);
            server.ClientConnected += (o, e) => Console.WriteLine("Client connected");

            while (true)
            {
                var overlappedClient = server.AcceptClient<OverlappedClient>();
                
            }
        }
        
        static void Main(string[] args)
        {
            Task.Run(() => ServerTask());

            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);
            var request = "GET / HTTP/1.1\r\nHost: google.com\r\n\r\n";
            var buffer = Encoding.UTF8.GetBytes(request);

            Task.Run(async () =>
            {
                var client = new AsyncClient();
                await client.ConnectAsync(endPoint);

                await client.SendAllAsync(buffer, buffer.Length);

                var buf = new byte[512];
                var received = await client.ReceiveAsync(buf, 0, buf.Length);
                var response = Encoding.UTF8.GetString(buf, 0, received);

                Console.WriteLine(response);
            });

            Process.GetCurrentProcess().WaitForExit();
        }

        private static object Benchmark(Action p)
        {
            throw new NotImplementedException();
        }
    }
}
