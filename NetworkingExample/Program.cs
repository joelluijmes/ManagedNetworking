using NetworkingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Events;

namespace NetworkingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("85.113.230.240"), 80);
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

    }
}
