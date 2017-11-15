using NExtends.Primitives.Types;
using RDD.Domain.Helpers;
using System.Collections;
using System.Linq;

namespace RDD.Web.Serialization
{
    public class StringEnumSerializer : PropertySerializer
    {
        public StringEnumSerializer() { }
        public StringEnumSerializer(IEntitySerializer serializer, IUrlProvider urlProvider) : base(serializer, urlProvider) { }

        public override object SerializeProperty(object entity, PropertySelector field)
        {
            var obj = base.SerializeProperty(entity, field);
            if (obj != null && field != null && field.EntityType != null && field.EntityType.IsEnum)
            {
                if (field.Lambda != null && field.Lambda.ReturnType.IsEnumerableOrArray())
                {
                    obj = ((IEnumerable)obj).Cast<object>().Select(o => o.ToString());
                }
                else
                {
                    obj = obj.ToString();
                }
            }
            return obj;
        }
    }
}