using RDD.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionSelectorParser<TClass>
    {
        public IExpressionSelector Parse(string input) => ParseChain(input);
        public IExpressionSelectorChain ParseChain(string input)
        {
            var tree = ParseTree(input);
            var chains = tree.ToList();
            if (chains.Count != 1)
            {
                throw new ArgumentException("Invalid input");
            }

            return chains.First();
        }

        public IExpressionSelectorChain ParseChain<TProp>(Expression<Func<TClass, TProp>> lambda) => ExpressionChainExtractor.AsExpressionSelectorChain(lambda);

        public IExpressionSelectorTree ParseTree(string input)
        {
            var tree = new TreeParser().Parse(input); 
            var result = new ExpressionSelectorTree();

            foreach (var subTree in tree.Children)
            {
                result.Children.Add(Parse(typeof(TClass), subTree));
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
            var property = classType.GetProperty(member, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            var returnType = property.PropertyType;

            var parameter = Expression.Parameter(classType);

            if (typeof(IDictionary).IsAssignableFrom(returnType))
            {
                var dictionaryKey = Expression.Constant(member);
                var itemsExpression = Expression.Property(parameter, "Item", dictionaryKey);

                return new ItemSelector { LambdaExpression = Expression.Lambda(itemsExpression, parameter) };
            }

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
