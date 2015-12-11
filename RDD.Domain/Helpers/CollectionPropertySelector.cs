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
			if (element.StartsWith("sum("))
			{
				//sum(myProp), sum(myProp, round), sum(myProp, ceiling, 3)
				var matches = Regex.Match(element, "sum\\(([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*)\\)").Groups;

				var propertyName = matches[1].Value;
				var property = GetEntityProperty(propertyName);

				var subParameters = new HashSet<string>();
				if (!String.IsNullOrEmpty(matches[2].Value))
				{
					subParameters.Add(matches[2].Value);
				}
				if (!String.IsNullOrEmpty(matches[3].Value))
				{
					subParameters.Add(matches[3].Value);
				}

				var sum = typeof(ISelection<>).GetMethod("Sum");

				var param = Expression.Parameter(EntityType, "p".Repeat(depth));

				var call = Expression.Call(param, sum, new Expression[] { Expression.Constant(property), Expression.Constant(subParameters.ToArray()) });

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
	}
}
