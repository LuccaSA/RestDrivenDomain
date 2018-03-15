using NExtends.Primitives.Strings;
using NExtends.Primitives.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RDD.Domain.Helpers
{
    public class DictionaryPropertySelector<TKey, TValue> : PropertySelector<Dictionary<TKey, TValue>>
    {
        public DictionaryPropertySelector(LambdaExpression expression)
            : base(expression) { }

        public override void Parse(string element, List<string> tail, int depth)
        {
            var param = Expression.Parameter(EntityType, "p".Repeat(depth));

            var dictionaryKey = Expression.Constant(element);
            var dictionaryAccessor = Expression.Property(param, "Item", new Expression[] { dictionaryKey });

            var lambda = Expression.Lambda(dictionaryAccessor, param);

            //Func<TSub> on s'intéresse à TSub
            var propertyType = EntityType.GetNeastedFuncType();

            //On regarde si le child n'existe pas déjà, auquel cas pas besoin de le recréer à chaque fois !
            Lambda = lambda;

            if (tail.Any())
            {
                Child = NewFromType(propertyType, null);
                Child.Parse(tail, depth + 1);
            }
        }
    }
}
