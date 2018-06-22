using RDD.Domain;
using RDD.Domain.Models.Querying;
using System.Collections;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public class WebQuery<TEntity> : Query<TEntity>
        where TEntity : class, IEntityBase
    {
        public WebQuery() : base() { }
        public WebQuery(params Filter<TEntity>[] filters)
            : base()
        {
            Filters = new FiltersConvertor<TEntity>().Convert(filters);
        }
        public WebQuery(Query<TEntity> source)
            : base(source) { }
    }
}
