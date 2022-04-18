using System;
using System.Data;

namespace TcpEventCommon
{
    /// <summary>
    /// Класс для отправки текстового сообщения и 
    /// информации о пересылаемых байтах следующих последними в потоке сетевых данных.
    /// </summary>
    [Serializable]
    public class SendInfo
    {
        public DataSet DataSet { get; set; } = new DataSet("Replication");
    }
}
