using RDD.Domain.Exceptions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Models.Convertors.Expressions
{
	class PropertyExpressionFactory : IPropertyExpressionFactory
	{
		public MemberExpression GetMemberExpression(Type type, ParameterExpression seed, string field, out PropertyInfo property)
		{
			var parts = field.Split('.');
			return GetMemberExpression(type, seed, parts, out property);
		}

		private MemberExpression GetMemberExpression(Type type, ParameterExpression seed, string[] fields, out PropertyInfo property)
		{
			property = null;
			MemberExpression body = null;

			foreach (var member in fields)
			{
				// Include internal properties through BindingFlags
				property = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					.FirstOrDefault(p => p.Name.ToLower() == member.ToLower());

				if (property == null)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Unknown property {0} on type {1}", member, type.Name));
				}

				if (!property.CanRead)
				{
					throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Property {0} of type {1} is set only", member, type.Name));
				}

				body = Expression.PropertyOrField((Expression)body ?? seed, member);
				type = property.PropertyType;
			}

			return body;
		}
	}
}