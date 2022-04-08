using System;

namespace TcpEventCommon
{
    [Serializable]
    public class AnswerData
    {
        public string UserName { get; set; }
        public DataKind Kind { get; set; }
        public Guid ID { get; set; }
        public User[] UserList { get; set; }
    }
}
