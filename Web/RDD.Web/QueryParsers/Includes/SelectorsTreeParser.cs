using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Helpers.Selectors;
using RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Web.QueryParsers.Includes
{
	public class SelectorsTreeParser
	{
		public SelectorsTreeRoot<T> ParseAllPropertie<T>()
		{
			var result = GetRoot<T>();

			foreach (var property in typeof(T).GetProperties())
			{
				result.AddChild(GetPropertySelector(GetType(result.Node.Expression), property));
			}
			return result;
		}

		public SelectorsTreeRoot<T> Parse<T>(Tree<string> input)
		{
			var result = GetRoot<T>();

			foreach (var child in input.Children)
			{
				result.AddChild(Parse(GetType(result.Node.Expression), child));
			}
			return result;
		}

		public IWritableSelectorsTree Parse(Type startingType, Tree<string> input)
		{
			var result = input.Node == TreeParser.Root ? GetRoot(startingType) : GetPropertySelector(startingType, GetProperty(startingType, input.Node));

			foreach (var child in input.Children)
			{
				result.AddChild(Parse(GetType(result.Node.Expression), child));
			}
			return result;
		}

		protected SelectorsTreeRoot<T> GetRoot<T>() => (SelectorsTreeRoot<T>)GetRoot(typeof(T));
		protected IWritableSelectorsTree GetRoot(Type startingType)
		{
			return (IWritableSelectorsTree)typeof(SelectorsTreeRoot<>).MakeGenericType(startingType).GetConstructor(new Type[] { }).Invoke(new object[] { });
		}
		
		protected IWritableSelectorsTree GetPropertySelector(Type type, PropertyInfo property)
		{
			if (typeof(IIncludable).IsAssignableFrom(property.PropertyType))
			{
				return (IWritableSelectorsTree)typeof(IncludableSelectorsTree<,>).MakeGenericType(type, property.PropertyType).GetConstructor(new[] { typeof(PropertyInfo) }).Invoke(new object[] { property });
			}
			else
			{
				return (IWritableSelectorsTree)typeof(PropertySelectorsTree<,>).MakeGenericType(type, property.PropertyType).GetConstructor(new[] { typeof(PropertyInfo) }).Invoke(new object[] { property });
			}
		}

		protected PropertyInfo GetProperty(Type type, string name)
		{
			return type.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase);
		}

		protected Type GetType(Expression expression)
		{
			return expression.Type.GetEnumerableOrArrayElementType().GetEnumerableOrArrayElementType();
		}
	}
}