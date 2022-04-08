using System;

namespace TcpEventServer
{
    /// <summary>
    /// Класс для отправки текстового сообщения и 
    /// информации о пересылаемых байтах следующих последними в потоке сетевых данных.
    /// </summary>
    [Serializable]
    public class SendInfo
    {
        public string Key;
        public string Value;
    }
}
