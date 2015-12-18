namespace NetworkingLibrary.Socks.SOCKS5
{
    public enum SocksAuthentication
    {
        NoAuthentication = 0x00,
        GSSAPI = 0x01,
        UserPass = 0x02,
        NonAcceptable = 0xFF
    }
}
