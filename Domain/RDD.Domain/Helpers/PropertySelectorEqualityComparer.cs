using System.Collections.Generic;

namespace RDD.Domain.Helpers
{
    public class PropertySelectorEqualityComparer : IEqualityComparer<PropertySelector>
    {
        public bool Equals(PropertySelector x, PropertySelector y)
        {
            if (x == null && y == null) { return true; }
            else if (x == null || y == null) { return false; }
            else
            {
                return x.EntityType == y.EntityType
                    && PropertySelector.AreEqual(x.Lambda, y.Lambda)
                    && Equals(x.Child, y.Child);
            }
        }

        public int GetHashCode(PropertySelector obj)
        {
            return obj.EntityType.GetHashCode() * 17
                + (obj.Lambda == null ? 0 : obj.Lambda.ReturnType.GetHashCode()) * 23
                + (obj.HasChild ? GetHashCode(obj.Child) : 0);
        }
    }
}
