using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace RDD.Web.Serialization
{
    public class CultureSerializer : PropertySerializer
    {
        public CultureSerializer()
        { }

        public CultureSerializer(IEntitySerializer serializer, IUrlProvider urlProvider)
            : base(serializer, urlProvider) { }

        public override Dictionary<string, object> SerializeProperties(object entity, PropertySelector fields)
        {
            Expression<Func<Culture, CultureInfo>> exp = c => c.RawCulture;

            fields.Remove(exp);

            return base.SerializeProperties(entity, fields);
        }
    }
}
