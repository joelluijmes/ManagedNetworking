using System.Net;
using NetworkingLibrary.Client;

using static System.Console;

namespace Echor
{
    class Program
    {
        static void Main(string[] args)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, 2222);

            var client = new UdpClient();
            client.Bind(localEndPoint);

            WriteLine($"Bound on {localEndPoint}");

            var buffer = new byte[2048];
            while (true)
            {
                EndPoint endPoint = new IPEndPoint(0, 0);
                var recv = client.ReceiveFrom(buffer, 0, buffer.Length, ref endPoint);
                WriteLine($"Received {recv} bytes from: {(IPEndPoint)endPoint}");

                var echoed = client.SendToAll(buffer, 0, recv, endPoint);
                WriteLine($"Echoed: {echoed}");
            }
        }
    }
}
