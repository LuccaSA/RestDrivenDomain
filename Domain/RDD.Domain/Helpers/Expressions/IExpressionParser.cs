using System;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions
{
    public interface IExpressionParser
    {
        IExpression Parse(Type classType, string input);
        IExpression Parse<TClass>(string input);

        IExpressionChain ParseChain(LambdaExpression lambda);
        IExpressionChain<TClass> ParseChain<TClass, TProp>(Expression<Func<TClass, TProp>> lambda);

        IExpressionChain ParseChain<TClass>(string input);
        IExpressionChain ParseChain(Type classType, string input);

        IExpressionTree<TClass> ParseTree<TClass, TProp>(Expression<Func<TClass, TProp>> lambda);
        IExpressionTree<TClass> ParseTree<TClass, TProp1, TProp2, TProp3, TProp4>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3, Expression<Func<TClass, TProp4>> lambda4);
        IExpressionTree<TClass> ParseTree<TClass, TProp1, TProp2, TProp3>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3);
        IExpressionTree<TClass> ParseTree<TClass, TProp1, TProp2>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2);
        IExpressionTree<TClass> ParseTree<TClass>(params LambdaExpression[] lambdas);
        IExpressionTree ParseTree(Type classType, string input);
        IExpressionTree<TClass> ParseTree<TClass>(string input);
    }
}