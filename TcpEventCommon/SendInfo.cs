using System;

namespace TcpEventCommon
{
    /// <summary>
    /// Класс для отправки текстового сообщения и 
    /// информации о пересылаемых байтах следующих последними в потоке сетевых данных.
    /// </summary>
    [Serializable]
    public class SendInfo
    {
        public Guid ClientID = Guid.Empty;
        public string Key;
        public string Value;
    }
}
