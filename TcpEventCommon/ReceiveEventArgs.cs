using System;

namespace TcpEventCommon
{
    public delegate void ReceiveEventHandler(object sender, ReceiveEventArgs e);

    /// <summary>
    /// Класс для передачи десериализированного контейнера при 
    /// возникновении события получения сетевых данных.
    /// </summary>
    public class ReceiveEventArgs : EventArgs
    {
        private readonly SendInfo sendinfo;

        public ReceiveEventArgs(SendInfo sendinfo)
        {
            this.sendinfo = sendinfo;
        }

        public SendInfo SendInfo
        {
            get { return sendinfo; }
        }

    }
}
