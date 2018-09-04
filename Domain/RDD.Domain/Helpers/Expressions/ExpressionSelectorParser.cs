using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionSelectorParser
    {
        public IExpressionSelector Parse(Type classType, string input) => ParseChain(classType, input);
        public IExpressionSelectorChain ParseChain(Type classType, string input)
        {
            var tree = ParseTree(classType, input);
            var chains = tree.ToList();
            if (chains.Count != 1)
            {
                throw new ArgumentException("Invalid input");
            }

            return chains.First();
        }

        public IExpressionSelectorChain ParseChain<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => ExpressionChainExtractor.AsExpressionSelectorChain(lambda);

        public IExpressionSelectorTree ParseTree<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => ParseTree(new LambdaExpression[] { lambda });
        public IExpressionSelectorTree ParseTree<TClass, TProp1, TProp2>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2)
            => ParseTree(new LambdaExpression[] { lambda1, lambda2 });
        public IExpressionSelectorTree ParseTree<TClass, TProp1, TProp2, TProp3>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3)
            => ParseTree(new LambdaExpression[] { lambda1, lambda2, lambda3 });

        public IExpressionSelectorTree ParseTree(params LambdaExpression[] lambdas)
        {
            var chains = lambdas.Select(lambda => ExpressionChainExtractor.AsExpressionSelectorChain(lambda));
           return new ExpressionSelectorTree { Children = ChainsToTree(chains).ToList() };
        }

        private IEnumerable<IExpressionSelectorTree> ChainsToTree(IEnumerable<IExpressionSelectorChain> chains)
        {
            if (chains == null)
            {
                return null;
            }

            return chains
                .Where(c => c != null)
                .GroupBy(c => c.Current, c => c.Next, new ExpressionSelectorEqualityComparer())
                .Select(g => new ExpressionSelectorTree { Node = g.Key, Children = ChainsToTree(g).ToList() });
        }

        public IExpressionSelectorTree ParseTree(Type classType, string input)
        {
            var tree = new TreeParser().Parse(input); 
            var result = new ExpressionSelectorTree();

            foreach (var subTree in tree.Children)
            {
                result.Children.Add(Parse(classType, subTree));
            }

            return result;
        }

        IExpressionSelectorTree Parse(Type classType, Tree<string> tree)
        {
            var selector = GetSelector(classType, tree.Node);
            return new ExpressionSelectorTree { Node = selector, Children = tree.Children.Select(c => Parse(selector.ResultType, c)).ToList() };
        }

        IExpressionSelector GetSelector(Type classType, string member)
        {
            var parameter = Expression.Parameter(classType);
            if (typeof(IDictionary).IsAssignableFrom(classType))
            {
                var dictionaryKey = Expression.Constant(member);
                var itemsExpression = Expression.Property(parameter, "Item", dictionaryKey);

                return new ItemSelector { LambdaExpression = Expression.Lambda(itemsExpression, parameter) };
            }

            var property = classType.GetProperty(member, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            var returnType = property.PropertyType;

            var propertyExpression = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            if (returnType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(returnType.GetGenericTypeDefinition()) && returnType != typeof(string))
            {
                return new EnumerableMemberSelector { LambdaExpression = lambda };
            }
            else
            {
                return new SimplePropertySelector { LambdaExpression = lambda };
            }
        }
    }
}
