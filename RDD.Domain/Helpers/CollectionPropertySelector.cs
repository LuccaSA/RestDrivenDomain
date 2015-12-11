using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
using RDD.Domain.Models;
using RDD.Domain.Exceptions;
using NExtends.Primitives;

namespace RDD.Domain.Helpers
{
	public class CollectionPropertySelector<TEntity> : PropertySelector<TEntity>
	{
		public CollectionPropertySelector()
			: base()
		{
			EntityType = typeof(ISelection<>).MakeGenericType(typeof(TEntity));
		}

		private PropertyInfo GetEntityProperty(string propertyName)
		{
			var property = typeof(TEntity).GetProperties().Where(p => p.Name.ToLower() == propertyName.ToLower()).FirstOrDefault();

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

				var param = Expression.Parameter(EntityType, "p".Repeat(depth));

				var call = GetExpressionCall(specialMethod, element, depth, param, property);
				
				var lambda = Expression.Lambda(call, param);

				var child = PropertySelector.NewFromType(EntityType);
				child.Lambda = lambda;
				child.Subject = propertyName;

				Children.Add(child);
			}
			else
			{
				base.Parse(element, tail, depth);
			}
		}

		private MethodCallExpression GetExpressionCall(string specialMethod, string pattern, int depth, ParameterExpression param, PropertyInfo property)
		{
			var rouding = DecimalRounding.Parse(specialMethod);

			var method = typeof(ISelection).GetMethod(specialMethod.ToFirstUpper(), new Type[] { typeof(PropertyInfo), typeof(DecimalRounding) });

			return Expression.Call(param, method, new Expression[] { Expression.Constant(property), Expression.Constant(rouding) });
		}
	}
}
