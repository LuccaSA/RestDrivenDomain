using NExtends.Primitives.Strings;
using System;

namespace Rdd.Domain.Helpers
{
    public class GuidHelper
    {
        public Guid Complete(string value)
        {
            //value : aabbccdd-eeff for instance
            value = value.Replace("-", "");

            var emptyGuid = "0".Repeat(32);

            var result = emptyGuid.Replace(emptyGuid.Substring(0, value.Length), value);

            return new Guid(result);
        }
    }
}
