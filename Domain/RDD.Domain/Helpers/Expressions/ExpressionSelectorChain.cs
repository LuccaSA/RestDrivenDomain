using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionSelectorChain : IExpressionSelectorChain
    {
        public IExpressionSelector Current { get; set; }

        public IExpressionSelectorChain Next { get; set; }

        public string Name => string.Join(".", new[] { Current?.Name, Next?.Name }.Where(e => !string.IsNullOrEmpty(e)));

        public Type ResultType => Next?.ResultType ?? Current.ResultType;

        public LambdaExpression ToLambdaExpression()
            => ExpressionChainer.Chain(Current?.ToLambdaExpression(), Next?.ToLambdaExpression());

        public static IExpressionSelectorChain New<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => new ExpressionSelectorParser().ParseChain(lambda);

        public bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property)
            => Contains(new ExpressionSelectorParser().ParseChain(property));

        public bool Contains(IExpressionSelectorChain chain)
        {
            return chain == null || (chain.Current.Equals(Current) && (chain.Next == null || (Next != null && Next.Contains(chain.Next))));
        }

        public bool Equals(IExpressionSelector other)
        {
            return (other == null && this == null)
                || (other != null && ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), ToLambdaExpression()));
        }

        public override string ToString() => Name;
    }
}
