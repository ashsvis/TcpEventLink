using System;

namespace TcpEventServer
{
    [Serializable]
    public class User
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public UserStatus Status { get; set; }

    }
}
