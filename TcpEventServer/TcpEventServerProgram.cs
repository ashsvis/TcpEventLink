using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.ServiceProcess;
using TcpEventCommon;

namespace TcpEventServer
{
    class TcpEventServerProgram
    {
        static TcpModule tcpmodule;

        static readonly Hashtable clients = new Hashtable();

        static void Main(string[] args)
        {
            #region Защита от повторного запуска
            var process = RunningInstance();
            if (process != null) return;
            #endregion

            tcpmodule = new TcpModule();
            tcpmodule.Receive += TcpModule_Receive;
            tcpmodule.Accept += TcpModule_Accept;
            tcpmodule.Disconnected += Tcpmodule_Disconnected;

            tcpmodule.StartServer();

            var tcptuning = new TcpTuning { Port = Global.SERVERTCPPORT };

            // если запускает пользователь сам
            if (Environment.UserInteractive)
            {

                Console.WriteLine("TcpEventServer. Ver. 0.1");
                Console.WriteLine("\nPress any key for exit...");
                Console.ReadKey();
            }
            else
            {
                // запуск в виде службы Windows
                var servicesToRun = new ServiceBase[] { new WinService() };
                ServiceBase.Run(servicesToRun);
            }
            tcpmodule.StopServer();
        }

        static void TcpModule_Receive(object sender, ReceiveEventArgs e)
        {
            if (Environment.UserInteractive)
                Console.WriteLine($"{e.SendInfo.Key}={e.SendInfo.Value}");
        }

        private static void TcpModule_Accept(object sender)
        {
            clients[sender] = new User();

            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Клиент подключился.");
            }
        }

        private static void Tcpmodule_Disconnected(object sender, string result)
        {
            var user = (User)clients[sender];
            if (user != null)
            {
                clients.Remove(sender);
            }
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(result);
            }
        }

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        private static Process RunningInstance()
        {
            var current = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(current.ProcessName);
            // Просматриваем все процессы
            return processes.Where(process => process.Id != current.Id).
                FirstOrDefault(process => Assembly.GetExecutingAssembly().
                    Location.Replace("/", "\\") == current.MainModule.FileName);
            // нет, таких процессов не найдено
        }
    }
}
