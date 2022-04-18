using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpEventCommon;

namespace TestEventClient
{
    class TestEventProgram
    {
        static Guid id = Guid.Empty;

        static void Main(string[] args)
        {
            var tcpmodule = new TcpModule();
            tcpmodule.Receive += Tcpmodule_Receive;
            tcpmodule.Disconnected += Tcpmodule_Disconnected;
            tcpmodule.Connected += Tcpmodule_Connected;
            tcpmodule.ConnectClient("127.0.0.1");
            string line;
            do
            {
                line = Console.ReadLine();
                tcpmodule.SendData("Клиент по месту:", line);
            } while (line != "exit");
            tcpmodule.DisconnectClient();
        }

        private static void Tcpmodule_Receive(object sender, ReceiveEventArgs e)
        {
            if (sender is TcpClientData client)
                Console.WriteLine($"{client.UserID}: {e.SendInfo.Key} = {e.SendInfo.Value}"); 
           
        }

        private static void Tcpmodule_Disconnected(object sender, string result)
        {
            Console.WriteLine("Отключился от сервера"); ;
        }

        private static void Tcpmodule_Connected(object sender, string result)
        {
            if (sender is TcpModule module)
            {
                id = module.GetLastClientId();
                Console.WriteLine($"Клиент {id} подключился к серверу");
            }
        }
    }
}
