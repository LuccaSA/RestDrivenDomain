using System;

namespace RDD.Domain.Helpers
{
    [Flags]
    public enum HttpVerb
    {
        None = 0,
        Get = 1 << 0,
        Post = 1 << 1,
        Put = 1 << 2,
        Delete = 1 << 3,

        All = Get | Post | Put | Delete
    }
}
