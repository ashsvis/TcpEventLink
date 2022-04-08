using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TcpEventCommon;

namespace TcpEventClient
{
    public delegate void GetUsersMethod(User[] users);

    public static class Client
    {
        public static IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Global.SERVERTCPPORT);
        public static int SendTimeout = 500;
        public static int ReceiveTimeout = 500;

        private static TcpModule module;

        public static string Status { get; set; }

        private static TcpModule tcpModule
        {
            get
            {
                if (module == null)
                {
                    module = new TcpModule();
                    module.Receive += Tcpmodule_Receive;
                    module.Disconnected += Tcpmodule_Disconnected;
                    module.Connected += Tcpmodule_Connected;
                }
                return module;
            }
        }

        private static void Tcpmodule_Connected(object sender, string result)
        {
            Status = result;
        }

        public static void Send(string key, string value)
        {
            tcpModule.SendNetworkData(key, value);
        }

        private static void Tcpmodule_Receive(object sender, ReceiveEventArgs e)
        {
            var key = e.SendInfo.Key;
            var value = e.SendInfo.Value;
            onReceive?.Invoke(sender, e);
        }

        private static event ReceiveEventHandler onReceive;

        public static event ReceiveEventHandler OnReceive
        {
            add
            {
                onReceive += value;
            }
            remove
            {
                onReceive -= value;
            }
        }

        private static void Tcpmodule_Disconnected(object sender, string result)
        {
            Status = result;
        }

        /// <summary>
        /// Получение массива байтов по запросу 
        /// </summary>
        /// <param name="request">Объект запроса</param>
        /// <returns>Массив байтов ответа</returns>
        public static byte[] GetData(RequestData request)
        {
            var result = new byte[] { };
            var remoteEp = ServerEndPoint;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.SendTimeout = SendTimeout;
                socket.ReceiveTimeout = ReceiveTimeout;
                try
                {
                    socket.Connect(remoteEp);
                    if (socket.Connected)
                    {
                        socket.Send(request.GetBytes());
                        result = ReturnAnswer(socket);
                        socket.Disconnect(false);
                        return result;
                    }
                }
                catch
                {
                    return result;
                }
            }
            return result;
        }

        private static byte[] ReturnAnswer(Socket socket)
        {
            var buff = new byte[8192];
            byte[] result = new byte[] { };
            try
            {
                var numBytes = socket.Receive(buff);
                if (numBytes > 0)
                {
                    var answer = new byte[numBytes];
                    Array.Copy(buff, answer, numBytes);
                    result = answer;
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                    if (Environment.UserInteractive) Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive) Console.WriteLine(ex.Message);
            }
            return result;
        }


        /// <summary>
        /// Получить список пользователей удалённо с сервера
        /// </summary>
        /// <param name="getUsersMethod">Метод для передачи результата асинхронной операции</param>
        public static void GetUsersAsync(GetUsersMethod getUsersMethod)
        {
            var ctx = TaskScheduler.FromCurrentSynchronizationContext();
            Task<User[]>.Factory.StartNew(() => { return GetUsers(); }).ContinueWith(t => getUsersMethod(t.Result), ctx);
        }

        /// <summary>
        /// Получить список пользователей удалённо с сервера
        /// </summary>
        /// <returns>Массив пользователей</returns>
        public static User[] GetUsers()
        {
            try
            {
                var data = GetData(new RequestData() { Kind = DataKind.UserList });
                var answer = ObjectCloner.SetData<AnswerData>(data);
                if (answer?.Kind != DataKind.UserList)
                    throw new InvalidCastException();
                return answer.UserList;
            }
            catch
            {
                return new User[] { };
            }
        }
    }
}
