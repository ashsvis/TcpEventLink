using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpEventClient;

namespace TestEventClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client.Connect("127.0.0.1");
            Client.Send("1", "2");
            Client.OnReceive += (o, e) => 
            {
                Console.WriteLine($"{e.SendInfo.Key} = {e.SendInfo.Key}");
            };

            Console.ReadKey();
            Client.Disconnect();
        }
    }
}
