using System.Net;

namespace TcpEventServer
{
    public class TcpTuning
    {
        public IPAddress Address { get; set; } = new IPAddress(new byte[] { 127, 0, 0, 1 });
        public int Port { get; set; } = Global.SERVERTCPPORT;
        public int SendTimeout { get; set; } = 5000;
        public int ReceiveTimeout { get; set; } = 5000;
    }
}
