using NetworkingLibrary.Socks.SOCKS5;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace SocksServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Socks5Server(2222);
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.InvalidClientConnected += Server_InvalidClientConnected;

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void Server_InvalidClientConnected(object sender, NetworkingLibrary.Events.ClientEventArgs e)
        {
            WriteLine($"Wrong client tried to connect from {e.Client.RemoteEndPoint}");
        }

        private static void Server_ClientDisconnected(object sender, NetworkingLibrary.Socks.Events.ClientEventArgs e)
        {
            WriteLine($"Client disconnected from {e.Client.RemoteEndPoint}");
        }

        private static void Server_ClientConnected(object sender, NetworkingLibrary.Socks.Events.ClientEventArgs e)
        {
            WriteLine($"Client connected from {e.Client.RemoteEndPoint}");
        }
    }
}
