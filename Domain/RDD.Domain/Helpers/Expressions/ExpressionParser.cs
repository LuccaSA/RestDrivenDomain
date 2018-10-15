using Rdd.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Domain.Helpers.Expressions
{
    public class ExpressionParser
    {
        public IExpression Parse<TClass>(string input)
            => ParseChain<TClass>(input);
        public IExpression Parse(Type classType, string input)
            => ParseChain(classType, input);

        public IExpressionChain ParseChain<TClass>(string input)
            => TreeToChain(ParseTree<TClass>(input));
        public IExpressionChain ParseChain(Type classType, string input)
            => TreeToChain(ParseTree(classType, input));

        private IExpressionChain TreeToChain(IExpressionTree tree)
        {
            var chains = tree.ToList();
            if (chains.Count != 1)
            {
                throw new ArgumentException("Invalid input");
            }

            return chains.First();
        }

        public IExpressionChain ParseChain(LambdaExpression lambda)
            => ExpressionChainExtractor.AsExpressionChain(lambda);

        public IExpressionChain<TClass> ParseChain<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => ExpressionChainExtractor.AsExpressionChain(lambda);

        public IExpressionTree<TClass> ParseTree<TClass, TProp>(Expression<Func<TClass, TProp>> lambda)
            => ParseTree<TClass>(new LambdaExpression[] { lambda });
        public IExpressionTree<TClass> ParseTree<TClass, TProp1, TProp2>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2)
            => ParseTree<TClass>(lambda1, lambda2);
        public IExpressionTree<TClass> ParseTree<TClass, TProp1, TProp2, TProp3>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3)
            => ParseTree<TClass>(lambda1, lambda2, lambda3);
        public IExpressionTree<TClass> ParseTree<TClass, TProp1, TProp2, TProp3, TProp4>(Expression<Func<TClass, TProp1>> lambda1, Expression<Func<TClass, TProp2>> lambda2, Expression<Func<TClass, TProp3>> lambda3, Expression<Func<TClass, TProp4>> lambda4)
            => ParseTree<TClass>(lambda1, lambda2, lambda3, lambda4);

        public IExpressionTree<TClass> ParseTree<TClass>(params LambdaExpression[] lambdas)
        {
            var chains = lambdas.Select(lambda => ExpressionChainExtractor.AsExpressionChain(lambda));
            return new ExpressionTree<TClass> { Children = ChainsToTree(chains).ToList() };
        }

        private IEnumerable<IExpressionTree> ChainsToTree(IEnumerable<IExpressionChain> chains)
            => chains?
                .Where(c => c != null)
                .GroupBy(c => c.Current, c => c.Next, new RddExpressionEqualityComparer())
                .Select(g => new ExpressionTree { Node = g.Key, Children = ChainsToTree(g).ToList() });

        public IExpressionTree ParseTree(Type classType, string input)
            => ParseTree(new ExpressionTree(), classType, input);

        public IExpressionTree<TClass> ParseTree<TClass>(string input)
            => ParseTree(new ExpressionTree<TClass>(), typeof(TClass), input);

        private TTree ParseTree<TTree>(TTree result, Type classType, string input)
            where TTree : ExpressionTree
        {
            var tree = new TreeParser().Parse(input);
            foreach (var subTree in tree.Children)
            {
                //skip node
                if (subTree.Node == "collection" && classType.GetProperty(subTree.Node) == null)
                {
                    var selectionType = typeof(ISelection<>).MakeGenericType(new[] { classType });
                    result.Children.AddRange(subTree.Children.Select(c => Parse(selectionType, c)));
                }
                else
                {
                    result.Children.Add(Parse(classType, subTree));
                }
            }

            return result;
        }

        IExpressionTree Parse(Type classType, Tree<string> tree)
        {
            var expression = GetExpression(classType, tree.Node);
            return new ExpressionTree { Node = expression, Children = tree.Children.Select(c => Parse(expression.ResultType, c)).ToList() };
        }

        IExpression GetExpression(Type classType, string member)
        {
            var parameter = Expression.Parameter(classType);
            if (typeof(IDictionary).IsAssignableFrom(classType))
            {
                var dictionaryKey = Expression.Constant(member);
                var itemsExpression = Expression.Property(parameter, "Item", dictionaryKey);

                return new ItemExpression { LambdaExpression = Expression.Lambda(itemsExpression, parameter) };
            }

            var property = GetPropertyInfo(classType, member);
            var returnType = property.PropertyType;

            var propertyExpression = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            if (typeof(IEnumerable).IsAssignableFrom(returnType) && returnType != typeof(string) && !typeof(IDictionary).IsAssignableFrom(returnType))
            {
                return new EnumerablePropertyExpression(lambda);
            }
            else
            {
                return new PropertyExpression(lambda);
            }
        }

        PropertyInfo GetPropertyInfo(Type type, string name)
        {
            if (type.IsInterface)
            {
                var considered = new HashSet<Type> { type };
                var stack = new Stack<Type>(new[] { type });

                while (stack.Any())
                {
                    var subType = stack.Pop();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Add(subInterface))
                        {
                            stack.Push(subInterface);
                        }
                    }

                    var property = subType.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        return property;
                    }
                }
                return null;
            }
            else
            {
                return type.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }
        }
    }
}