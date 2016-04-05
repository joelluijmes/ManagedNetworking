using System.Net;
using System.Text;
using NetworkingLibrary.Client;

using static System.Console;
using System.Linq;

namespace ExampleClient
{
    class Program
    {
        private static TcpClient _client;

        static void Main(string[] args)
        {
            _client = new TcpClient();
            StartSending();
        }
        
        private static void StartSending()
        {
            _client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1111));

            WriteLine("Enter d to disconnect, q to abort");
            while (true)
            {
                Write("Send: ");
                var data = ReadLine();
                if (string.IsNullOrEmpty(data))
                    continue;

                if (data.Length == 1)
                {
                    switch (data[0])
                    {
                        case 'd':
                            _client.Disconnect();
                            return;
                        case 'q':
                            _client = null;
                            return;
                    }
                }

                var bytes = Encoding.UTF8.GetBytes(data);
                _client.SendAll(bytes);
            }
        }
    }
}
