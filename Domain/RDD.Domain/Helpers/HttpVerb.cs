using System;

namespace RDD.Domain.Helpers
{
    [Flags]
    public enum HttpVerbs
    {
        None = 0,
        Get = 0x1,
        Post = 0x2,
        Put = 0x4,
        Delete = 0x8,
        All = Get | Post | Put | Delete
    }
}
