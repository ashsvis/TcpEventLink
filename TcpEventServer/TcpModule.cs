using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TcpEventServer
{
    /// <summary>
    /// Класс способный выступать в роли сервера или клиента в TCP соединении.
    /// Отправляет и получает по сети файлы и текстовые сообщения.
    /// </summary>
    public class TcpModule
    {

        #region Определение событий сетевого модуля

        // Типы делегатов для обработки событий в паре с соответствующим объектом события.

        // Обработчики события акцептирования (принятия клиентов) прослушивающим сокетом
        public delegate void AcceptEventHandler(object sender);
        public event AcceptEventHandler Accept;

        // Обработчики события подключения клиента к серверу
        public delegate void ConnectedEventHandler(object sender, string result);
        public event ConnectedEventHandler Connected;

        // Обработчики события отключения конечных точек (клиентов или сервера)
        public delegate void DisconnectedEventHandler(object sender, string result);
        public event DisconnectedEventHandler Disconnected;

        // Обработчики события извлечения данных 
        public delegate void ReceiveEventHandler(object sender, ReceiveEventArgs e);
        public event ReceiveEventHandler Receive;

        #endregion


        #region Объявления членов класса

        // Прослушивающий сокет для работы модуля в режиме сервера TCP
        private TcpListener tcpListener;

        private readonly List<TcpClientData> listClients = new List<TcpClientData>();

        // Удобный контейнер для подключенного клиента.
        private TcpClientData TcpClient
        {
            get
            {
                if (listClients.Count == 0)
                    return null;
                return listClients[listClients.Count - 1];
            }
            set
            {
                if (!listClients.Contains(value))
                    listClients.Add(value);
            }
        }

        /// <summary>
        /// Возможные режимы работы TCP модуля
        /// </summary>
        public enum Mode { Indeterminately, Server, Client };

        /// <summary>
        /// Режим работы TCP модуля
        /// </summary>
        public Mode modeNetwork;

        #endregion


        #region Интерфейсная часть TCP модуля

        /// <summary>
        /// Запускает сервер, прослушивающий все IP адреса, и одновременно
        /// метод асинхронного принятия (акцептирования) клиентов.
        /// </summary>
        public void StartServer()
        {
            if (modeNetwork == Mode.Indeterminately)
            {
                try
                {
                    tcpListener = new TcpListener(IPAddress.Any, Global.SERVERTCPPORT);
                    tcpListener.Start();
                    tcpListener.BeginAcceptTcpClient(AcceptCallback, tcpListener);
                    modeNetwork = Mode.Server;
                }
                catch
                {
                    tcpListener = null;
                }
            }
        }


        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void StopServer()
        {
            if (modeNetwork == Mode.Server)
            {
                modeNetwork = Mode.Indeterminately;
                tcpListener.Stop();
                tcpListener = null;
                DeleteClient(TcpClient);
            }
        }


        /// <summary>
        /// Попытка асинхронного подключения клиента к серверу
        /// </summary>
        /// <param name="ipserver">IP адрес сервера</param>
        public void ConnectClient(string ipserver)
        {
            if (modeNetwork == Mode.Indeterminately)
            {
                TcpClient = new TcpClientData();
                TcpClient.tcpClient.BeginConnect(IPAddress.Parse(ipserver), Global.SERVERTCPPORT, new AsyncCallback(ConnectCallback), TcpClient);
                modeNetwork = Mode.Client;
            }
        }


        /// <summary>
        /// Отключение клиента от сервера
        /// </summary>
        public void DisconnectClient()
        {
            if (modeNetwork == Mode.Client)
            {
                modeNetwork = Mode.Indeterminately;
                DeleteClient(TcpClient);
            }
        }

        /// <summary>
        /// Завершение работы подключенного клиента
        /// </summary>
        private void DeleteClient(TcpClientData mtc)
        {
            if (mtc != null && mtc.tcpClient.Connected == true)
            {
                mtc.tcpClient.GetStream().Close();  // по настоянию MSDN закрываем поток отдельно у клиента
                mtc.tcpClient.Close();              // затем закрываем самого клиента
                listClients.Remove(mtc);
            }
        }

        /// <summary>
        /// Метод упрощенного создания заголовка с информацией о размере данных отправляемых по сети.
        /// </summary>
        /// <param name="length">длина данных подготовленных для отправки по сети</param>
        /// <returns>возращает байтовый массив заголовка</returns>
        private byte[] GetHeader(int length)
        {
            string header = length.ToString();
            if (header.Length < 9)
            {
                string zeros = null;
                for (int i = 0; i < (9 - header.Length); i++)
                {
                    zeros += "0";
                }
                header = zeros + header;
            }

            byte[] byteheader = Encoding.Default.GetBytes(header);

            return byteheader;
        }

        private void SendNetworkData(string key, string value)
        {
            // Состав отсылаемого универсального сообщения
            // 1. Заголовок о следующим объектом класса подробной информации дальнейших байтов
            // 2. Объект класса подробной информации о следующих байтах
            // 3. Байты непосредственно готовых к записи в файл или для чего-то иного.

            var si = new SendInfo
            {
                Key = key,
                Value = value
            };

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, si);
            ms.Position = 0;
            byte[] infobuffer = new byte[ms.Length];
            int r = ms.Read(infobuffer, 0, infobuffer.Length);
            ms.Close();

            byte[] header = GetHeader(infobuffer.Length);
            byte[] total = new byte[header.Length + infobuffer.Length];

            Buffer.BlockCopy(header, 0, total, 0, header.Length);
            Buffer.BlockCopy(infobuffer, 0, total, header.Length, infobuffer.Length);

            // Отправим данные подключенным клиентам
            foreach (var client in listClients.Where(item => item.tcpClient.Connected))
            {
                NetworkStream ns = client.tcpClient.GetStream();
                // Так как данный метод вызывается в отдельном потоке рациональней использовать синхронный метод отправки
                ns.Write(total, 0, total.Length);
            }

            // Обнулим все ссылки на многобайтные объекты и попробуем очистить память
            header = null;
            infobuffer = null;
            total = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Подтверждение успешной отправки
        }


        /// <summary>
        /// Универсальный метод останавливающий работу сервера и закрывающий все сокеты
        /// вызывается в событии закрытия родительской формы.
        /// </summary>
        public void CloseSocket()
        {
            StopServer();
            DisconnectClient();
        }

        #endregion


        #region Асинхронные методы сетевой работы TCP модуля


        /// <summary>
        /// Обратный метод завершения принятия клиентов
        /// </summary>
        public void AcceptCallback(IAsyncResult ar)
        {
            if (modeNetwork == Mode.Indeterminately) return;

            TcpListener listener = (TcpListener)ar.AsyncState;
            try
            {
                TcpClient = new TcpClientData();
                TcpClient.tcpClient = listener.EndAcceptTcpClient(ar);

                // Немедленно запускаем асинхронный метод извлечения сетевых данных
                // для акцептированного TCP клиента
                NetworkStream ns = TcpClient.tcpClient.GetStream();
                TcpClient.buffer = new byte[Global.LENGTHHEADER];
                ns.BeginRead(TcpClient.buffer, 0, TcpClient.buffer.Length, new AsyncCallback(ReadCallback), TcpClient);

                // Продолжаем ждать запросы на подключение
                listener.BeginAcceptTcpClient(AcceptCallback, listener);

                // Активация события успешного подключения клиента
                if (Accept != null)
                {
                    Accept.BeginInvoke(TcpClient /*this*/, null, null);
                }
            }
            catch
            {
                // Обработка исключительных ошибок возникших при акцептирования клиента.
                //SoundError();
            }
        }


        /// <summary>
        /// Метод вызываемый при завершении попытки поключения клиента
        /// </summary>
        public void ConnectCallback(IAsyncResult ar)
        {
            string result = "Подключение успешно.";
            try
            {
                // Получаем подключенного клиента
                TcpClientData myTcpClient = (TcpClientData)ar.AsyncState;
                NetworkStream ns = myTcpClient.tcpClient.GetStream();
                myTcpClient.tcpClient.EndConnect(ar);

                // Запускаем асинхронный метод чтения сетевых данных для подключенного TCP клиента
                myTcpClient.buffer = new byte[Global.LENGTHHEADER];
                ns.BeginRead(myTcpClient.buffer, 0, myTcpClient.buffer.Length, new AsyncCallback(ReadCallback), myTcpClient);

            }
            catch (Exception)
            {
                DisconnectClient();
                result = "Подключение не успешно.";
            }

            // Активация события успешного или неуспешного подключения к серверу,
            // здесь серверу можно отослать ознакомительные данные о себе (например, имя клиента)
            if (Connected != null)
                Connected.BeginInvoke(this, result, null, null);
        }


        /// <summary>
        /// Метод асинхронно вызываемый при наличие данных в буферах приема.
        /// </summary>

        public void ReadCallback(IAsyncResult ar)
        {
            if (modeNetwork == Mode.Indeterminately) return;

            TcpClientData myTcpClient = (TcpClientData)ar.AsyncState;

            try
            {
                NetworkStream ns = myTcpClient.tcpClient.GetStream();

                int r = ns.EndRead(ar);

                if (r > 0)
                {
                    // Из главного заголовка получим размер массива байтов информационного объекта
                    string header = Encoding.Default.GetString(myTcpClient.buffer);
                    int leninfo = int.Parse(header);

                    // Получим и десериализуем объект с подробной информацией о содержании получаемого сетевого пакета
                    MemoryStream ms = new MemoryStream(leninfo);
                    byte[] temp = new byte[leninfo];
                    r = ns.Read(temp, 0, temp.Length);
                    ms.Write(temp, 0, r);
                    BinaryFormatter bf = new BinaryFormatter();
                    ms.Position = 0;
                    SendInfo sc = (SendInfo)bf.Deserialize(ms);
                    ms.Close();

                    Receive?.Invoke(myTcpClient, new ReceiveEventArgs(sc));

                    myTcpClient.buffer = new byte[Global.LENGTHHEADER];
                    ns.BeginRead(myTcpClient.buffer, 0, myTcpClient.buffer.Length, new AsyncCallback(ReadCallback), myTcpClient);
                }
                else
                {
                    DeleteClient(myTcpClient);
                    // Событие клиент отключился
                    if (Disconnected != null)
                        Disconnected.BeginInvoke(myTcpClient, "Клиент отключился.", null, null);
                }
            }
            catch (Exception)
            {
                DeleteClient(myTcpClient);
                // Событие клиент отключился
                if (Disconnected != null)
                    Disconnected.BeginInvoke(myTcpClient, "Клиент отключился аварийно.", null, null);
            }

        }

        #endregion

    }
}
