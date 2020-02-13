using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Infra.Tests
{
    public class GenericKey
    {
        public long Sum { get; set; }
        public string BaseProperty { get; set; }
    }

    public class TypeOneKey : GenericKey
    {
        public string SuperProperty { get; set; }
    }

    public class Metric<TKey>
        where TKey : GenericKey
    {
        public long Sum { get; set; }
        public TKey Key { get; set; }
    }

    public class TypeOneMetric : Metric<TypeOneKey>
    {
        public long TypeOneProperty { get; set; }
    }

    public class HierarchyTests : DatabaseTest
    {
        [Fact]
        public async Task OrderByTest()
        {
            await RunCodeInsideIsolatedDatabaseAsync((context) =>
            {
                using (var newContext = GetContext())
                {
                    newContext.Set<Super>().Add(new Super
                    {
                        BaseProperty = "a",
                        Value = 1
                    });
                    newContext.Set<Super>().Add(new Super
                    {
                        BaseProperty = "a",
                        Value = 2
                    });
                    newContext.Set<Super>().Add(new Super
                    {
                        BaseProperty = "b",
                        Value = 2
                    });
                    newContext.SaveChanges();

                    var query = newContext.Set<Super>().GroupBy(e => new
                    {
                        e.BaseProperty,
                        e.SuperProperty
                    })
                    .Select(g => new TypeOneMetric
                    {
                        Sum = g.Sum(e => e.Value),
                        Key = new TypeOneKey
                        {
                            Sum = g.Sum(e => e.Value2),
                            BaseProperty = g.Key.BaseProperty,
                            SuperProperty = g.Key.SuperProperty
                        }
                    });

                    var set = new Service<TypeOneMetric, TypeOneKey>().ApplyOrderBys(query);

                    var response = set.ToList();
                }

                return Task.CompletedTask;
            });
        }
    }

    public class Service<TMetric, TKey> where TMetric : Metric<TKey>
        where TKey : GenericKey
    {
        public IQueryable<TMetric> ApplyOrderBys(IQueryable<TMetric> entities)
        {
            var expression = new ExpressionParser().Parse<TMetric>("Sum");
            var orderBy = new OrderBy<TMetric>(expression.ToLambdaExpression());
            return orderBy.ApplyOrderBy(entities);
        }
    }
}