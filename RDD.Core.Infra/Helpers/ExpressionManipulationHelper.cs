using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Helpers
{
	public class ExpressionManipulationHelper
	{
		// Utile pour les types créés à partir de Group By d'un autre type
		public static Expression<Func<TOut, bool>> ApplyExpressionToParameterType<TIn, TOut>(Expression<Func<TIn, bool>> original)
		{
			return ApplyExpressionToParameterType<TIn, TOut>(original, new Dictionary<string, string>());
		}
		public static Expression<Func<TOut, bool>> ApplyExpressionToParameterType<TIn, TOut>(Expression<Func<TIn, bool>> original, Dictionary<string, string> propertyRenaming)
		{
			var newParam = Expression.Parameter(typeof(TOut), "pOut");
			var visitor = new ParameterReplacementVisitor(original.Parameters[0], newParam, propertyRenaming);
			var newParentBody = visitor.Visit(original.Body);
			var exprOut = Expression.Lambda<Func<TOut, bool>>(newParentBody, newParam);
			return exprOut;
		}

		public class ParameterReplacementVisitor : ExpressionVisitor
		{
			private readonly Expression _oldParameter;
			private readonly Expression _newParameter;
			private readonly Dictionary<string, string> _propertyRenaming;

			public ParameterReplacementVisitor(ParameterExpression oldParameter, ParameterExpression newParameter, Dictionary<string, string> propertyRenaming)
			{
				_oldParameter = oldParameter;
				_newParameter = newParameter;
				_propertyRenaming = propertyRenaming;
			}
			protected override Expression VisitParameter(ParameterExpression node)
			{
				if (_oldParameter == node)
				{
					return _newParameter;
				}
				else
				{
					return base.VisitParameter(node);
				}
			}
			protected override Expression VisitMember(MemberExpression node)
			{
				if (node.Member.DeclaringType == _oldParameter.Type)
				{
					var propertyName = node.Member.Name;
					if (_propertyRenaming.ContainsKey(propertyName))
					{
						propertyName = _propertyRenaming[propertyName];
					}
					var propertyInfo = _newParameter.Type.GetProperty(propertyName);
					return Expression.MakeMemberAccess(_newParameter, propertyInfo);
				}
				else
				{
					return base.VisitMember(node);
				}
			}
		}
		public static Expression BuildAny<TSource>(Expression accessor, Expression predicate)
		{
			return BuildAny(typeof(TSource), accessor, predicate);
		}
		public static Expression BuildAny(Type tSource, Expression accessor, Expression predicate)
		{
			return BuildEnumerableMethod(tSource, accessor, predicate, "Any");
		}

		public static Expression BuildWhere<TSource>(Expression accessor, Expression predicate)
		{
			return BuildWhere(typeof(TSource), accessor, predicate);
		}
		public static Expression BuildWhere(Type tSource, Expression accessor, Expression predicate)
		{
			return BuildEnumerableMethod(tSource, accessor, predicate, "Where");
		}

		private static Expression BuildEnumerableMethod(Type tSource, Expression accessor, Expression predicate, string method)
		{
			var overload = typeof(Enumerable).GetMethods()
									  .Single(mi => mi.Name == method && mi.GetParameters().Count() == 2).MakeGenericMethod(tSource);

			var call = Expression.Call(
				overload,
				accessor,
				predicate);

			return call;
		}

		public static bool HasSubexpressionsOfType<TTest>(Expression original)
		{
			var visitor = new HasTypeVisitor<TTest>();
			return visitor.VisitRoot(original);
		}

		private class HasTypeVisitor<TKeep> : ExpressionVisitor
		{
			private Type _toKeep;
			private bool _HasType = false;
			public HasTypeVisitor()
			{
				_toKeep = typeof(TKeep);
			}
			public bool VisitRoot(Expression node)
			{
				var visited = Visit(node);
				return _HasType;
			}
			public override Expression Visit(Expression node)
			{
				if (_HasType) { return node; }
				if (node != null && node.Type == _toKeep)
				{
					_HasType = _HasType | true;
					return node;
				}
				else
				{
					return base.Visit(node);
				}
			}
		}
	}
}
