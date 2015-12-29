using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkingLibrary.Socks.SOCKS5;

namespace SocksClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);
            var wikiPoint = new IPEndPoint(IPAddress.Parse("164.138.24.109"), 80);
            var client = new Socks5Client(endPoint);
            client.InitiateConnection(wikiPoint).Wait();

            var request = "GET / HTTP/1.1\r\nHost: 164.138.24.109\r\n\r\n";
            client.SendAll(Encoding.UTF8.GetBytes(request));

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}
