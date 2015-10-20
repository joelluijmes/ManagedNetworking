using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Events
{
    public class TransferEventArgs
    {
        public OverlappedClient Client { get; }
        public byte[] Bytes { get; }
        public int Count { get; }

        public TransferEventArgs(OverlappedClient client, byte[] bytes, int count)
        {
            Client = client;
            Bytes = bytes;
            Count = count;
        }
    }
}
