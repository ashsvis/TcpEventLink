using System;

namespace TcpEventCommon
{
    [Serializable]
    public class User
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public UserStatus Status { get; set; }

    }
}
