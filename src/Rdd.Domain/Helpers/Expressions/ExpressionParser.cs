using Microsoft.Extensions.Primitives;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Domain.Helpers.Expressions
{
    public class ExpressionParser : IExpressionParser
    {
        public IExpression Parse<TClass>(string input)
            => ParseChain<TClass>(input);
        public IExpression Parse(Type classType, string input)
            => ParseChain(classType, input);

        public IExpressionChain ParseChain<TClass>(string input)
            => ParseChain<ExpressionChain<TClass>>(typeof(TClass), new Queue<StringSegment>(new StringTokenizer(input, new[] { '.' })));
        public IExpressionChain ParseChain(Type classType, string input)
            => ParseChain<ExpressionChain>(classType, new Queue<StringSegment>(new StringTokenizer(input, new[] { '.' })));

        private TChain ParseChain<TChain>(Type classType, Queue<StringSegment> tokens)
            where TChain : ExpressionChain, new()
        {
            if (tokens.Count == 0)
            {
                return null;
            }

            var expression = GetExpression(classType, tokens.Dequeue().Value);
            return new TChain { Current = expression, Next = ParseChain<ExpressionChain>(expression.ResultType, tokens) };
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

        private IExpressionTree Parse(Type classType, Tree<string> tree)
        {
            var expression = GetExpression(classType, tree.Node);
            return new ExpressionTree { Node = expression, Children = tree.Children.Select(c => Parse(expression.ResultType, c)).ToList() };
        }

        private IExpression GetExpression(Type classType, string member)
        {
            var parameter = Expression.Parameter(classType);
            if (typeof(IDictionary).IsAssignableFrom(classType))
            {
                var dictionaryKey = Expression.Constant(member);
                var itemsExpression = Expression.Property(parameter, "Item", dictionaryKey);

                return new ItemExpression { LambdaExpression = Expression.Lambda(itemsExpression, parameter) };
            }

            var property = GetPropertyInfo(classType, member);
            if (property == null)
            {
                throw new BadRequestException($"Selected property {member} does not exist on objet of type {classType.Name}");
            }

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

        private PropertyInfo GetPropertyInfo(Type type, string name)
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
                var property = type.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                
                //Ef needs the property coming from the declaring type
                if (property != null && property.ReflectedType != property.DeclaringType)
                {
                    property = property.DeclaringType.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                }
                return property;
            }
        }
    }
}