using RDD.Domain.Helpers;
using System.Collections;

namespace RDD.Web.Querying
{
    public class WebFilter<TEntity>
    {
        public PropertySelector<TEntity> Property { get; }
        public WebFilterOperand Operand { get; }
        public IList Values { get; }

        public WebFilter(PropertySelector<TEntity> property, WebFilterOperand operand, IList values)
        {
            Property = property;
            Operand = operand;
            Values = values;
        }
    }
}