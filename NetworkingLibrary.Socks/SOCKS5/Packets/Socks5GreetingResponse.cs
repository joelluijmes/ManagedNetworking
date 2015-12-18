namespace NetworkingLibrary.Socks.Packets
{
    internal sealed class Socks5GreetingResponse
    {
        public byte Version => 0x05;
        public SocksAuthentication AuthenticationMethod { get; private set; }

        public Socks5GreetingResponse(SocksAuthentication authentication)
        {
            AuthenticationMethod = authentication;
        }
    }
}
