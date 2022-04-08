using System;

namespace TcpEventCommon
{
    [Serializable]
    public enum DataKind
    {
        Login,
        ID,
        UserList,
        Message,
        Logout
    }
}
