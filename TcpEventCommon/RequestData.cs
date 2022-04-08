using System;

namespace TcpEventCommon
{
    [Serializable]
    public class RequestData
    {
        public string UserName { get; set; }
        public UserStatus UserStatus { get; set; }
        public DataKind Kind { get; set; }
        public Guid ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
