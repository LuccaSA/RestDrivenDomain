using NExtends.Primitives;
using RDD.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RDD.Domain.Helpers
{
	public class CollectionPropertySelector<TEntity> : PropertySelector<TEntity>
	{
		public CollectionPropertySelector()
		{
			EntityType = typeof(ISelection<>).MakeGenericType(typeof(TEntity));
		}

		private PropertyInfo GetEntityProperty(string propertyName)
		{
			var property = typeof(TEntity).GetProperties().FirstOrDefault(p => p.Name.ToLower() == propertyName.ToLower());

			if (property == null)
			{
				throw new HttpLikeException(System.Net.HttpStatusCode.BadRequest, String.Format("Unknown property {0} on type ISelection", propertyName));
			}

			return property;
		}

		public override void Parse(string element, List<string> tail, int depth)
		{
			var specialMethods = new HashSet<string>() { "sum", "min", "max" };

			if (specialMethods.Any(m => element.StartsWith(m)))
			{
				var specialMethod = element.StartsWith("sum") ? "sum" : element.StartsWith("min") ? "min" : "max";
				var matches = Regex.Match(element, String.Format("{0}\\(([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*)\\)", specialMethod)).Groups;
				var propertyName = matches[1].Value;
				var property = GetEntityProperty(propertyName);
				var rouding = DecimalRounding.Parse(element);
				var param = Expression.Parameter(EntityType, "p".Repeat(depth));

				var call = GetExpressionCall(specialMethod, rouding, depth, param, property);
				
				var lambda = Expression.Lambda(call, param);

				var child = NewFromType(EntityType);
				child.Lambda = lambda;
				child.Subject = propertyName;

				Children.Add(child);
			}
			else
			{
				base.Parse(element, tail, depth);
			}
		}

		private MethodCallExpression GetExpressionCall(string specialMethod, DecimalRounding rouding, int depth, ParameterExpression param, PropertyInfo property)
		{
			var method = typeof(ISelection).GetMethod(specialMethod.ToFirstUpper(), new[] { typeof(PropertyInfo), typeof(DecimalRounding) });

			return Expression.Call(param, method, new Expression[] { Expression.Constant(property), Expression.Constant(rouding) });
		}
	}
}
