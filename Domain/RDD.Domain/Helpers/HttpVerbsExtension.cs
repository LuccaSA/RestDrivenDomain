using System.Runtime.CompilerServices;

namespace RDD.Domain.Helpers
{
    public static class HttpVerbsExtension
    {
        /// <summary>
        /// Check if HttpVerbs [Flags] enum have a specific HttpVerbs value
        /// Should be replace with HasFlag when netcore2.1 will be out
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasVerb(this HttpVerbs verb, HttpVerbs flag) => (verb & flag) == flag;
    }
}