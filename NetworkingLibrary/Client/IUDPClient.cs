using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLibrary.Client
{
    public interface IUdpClient
    {
        void Bind(EndPoint endPoint);
        int SendTo(byte[] buffer, int offset, int count, EndPoint endPoint);
        bool SendToAll(byte[] buffer, int count, EndPoint endPoint);
        int ReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint endPoint);
        bool ReceiveFromAll(byte[] buffer, int count, ref EndPoint endPoint);
    }
}
