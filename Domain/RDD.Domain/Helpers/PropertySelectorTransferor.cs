using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Helpers
{
	/// <summary>
	/// Ce visiteur permet de trouver et transférer les expression d'un TEntity vers un TSub
	/// Etant donné que le TSub est une propriété du TEntity
	/// </summary>
	public class PropertySelectorTransferor<TEntity, TSub> : PropertySelectorTransferor
	{
		public PropertySelectorTransferor(string propertyName)
			: base(typeof(TEntity), typeof(TSub), propertyName) { }
	}
	public class PropertySelectorTransferor : ExpressionVisitor
	{
		private Type _entityType;
		private Type _subType;
		private PropertyInfo _property;
		private ParameterExpression _param;

		public PropertySelectorTransferor(Type entityType, Type subType, string propertyName)
		{
			_entityType = entityType;
			_subType = subType;

			_param = Expression.Parameter(_subType, "p");

			_property = _entityType
				.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.FirstOrDefault(p => p.Name.ToLower() == propertyName.ToLower());

			if (_property == null)
			{
				throw new Exception(String.Format("Property {0} of type {1} does not exist on type {2}", propertyName, _subType.Name, _entityType.Name));
			}
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			var subjectType = typeof(T).GetGenericArguments()[0];

			if (subjectType == _entityType)
			{
				ParameterExpression param;
				var body = VisitBody(node.Body, node, out param);

				return Expression.Lambda(body, param);
			}

			throw new NotImplementedException();
		}

		protected Expression VisitBody(Expression node, Expression caller, out ParameterExpression param)
		{
			switch (node.NodeType)
			{
				case ExpressionType.MemberAccess:
					{
						param = _param;
						node = VisitMember(node as MemberExpression);
						break;
					}
				case ExpressionType.Call:
					{
						node = VisitMethodCall(node as MethodCallExpression, out param);
						break;
					}
				case ExpressionType.Convert:
					{
						param = _param;
						node = VisitMember((node as UnaryExpression).Operand as MemberExpression);
						break;
					}
				default:
					throw new NotImplementedException();
			}

			return node;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			//d => d.Head.LegalEntity.Name
			//On est sur l'accès à LegalEntity
			//On retourne p.LegalEntity.Name
			if (node.Expression.NodeType == ExpressionType.MemberAccess)
			{
				var property = (PropertyInfo)(node.Expression as MemberExpression).Member;

				if (property == _property)
				{
					return Expression.Property(_param, (PropertyInfo)node.Member);
				}
			}

			return base.VisitMember(node);
		}

		protected Expression VisitMethodCall(MethodCallExpression node, out ParameterExpression param)
		{
			var caller = node.Arguments[0] as MemberExpression;
			var property = (PropertyInfo)caller.Member;
			var lambda = node.Arguments[1] as LambdaExpression;

			//d => d.Users.Select(u => u.LegalEntity.Name)
			//On est sur le Select
			//On retourne u.LegalEntity.Name après l'avoir visité, càd transformé en p.LegalEntity.Name
			if (property == _property)
			{
				param = lambda.Parameters[0];
				return lambda.Body;
			}

			//u => u.Department.Users.Select(pp => pp.Name)
			//On est sur le Select
			//On retourne Users.Select(pp => pp.Name), donc le même Select mais en ayant visité son caller
			param = _param;
			caller = (MemberExpression)VisitMember(caller);

			return Expression.Call(null, node.Method, caller, lambda);
		}
	}
}
