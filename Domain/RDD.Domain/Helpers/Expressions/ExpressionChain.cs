using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionChain : IExpressionChain
    {
        public IExpression Current { get; set; }

        public IExpressionChain Next { get; set; }

        public string Name => string.Join(".", new[] { Current?.Name, Next?.Name }.Where(e => !string.IsNullOrEmpty(e)));

        public Type ResultType => Next?.ResultType ?? Current.ResultType;

        public LambdaExpression ToLambdaExpression()
            => ExpressionChainer.Chain(Current?.ToLambdaExpression(), Next?.ToLambdaExpression());

        public bool Contains<TClass, TProp>(Expression<Func<TClass, TProp>> property)
            => Contains(new ExpressionParser().ParseChain(property));

        public bool Contains(IExpressionChain chain)
        {
            return chain == null || (chain.Current.Equals(Current) && (chain.Next == null || (Next != null && Next.Contains(chain.Next))));
        }

        public virtual bool Equals(IExpression other)
            => other != null && Utils.ExpressionEqualityComparer.Eq(other.ToLambdaExpression(), ToLambdaExpression());

        public override string ToString() => Name;
    }

    public class ExpressionChain<TClass> : ExpressionChain, IExpressionChain<TClass>
    {
        public static IExpressionChain<TClass> New<TProp>(Expression<Func<TClass, TProp>> lambda)
            => new ExpressionParser().ParseChain(lambda);

        public bool Contains<TProp>(Expression<Func<TClass, TProp>> property)
            => base.Contains(property);
    }
}
