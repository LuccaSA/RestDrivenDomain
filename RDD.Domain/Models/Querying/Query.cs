using RDD.Infra;
using RDD.Infra.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models.Querying
{
	public class Query<TEntity> : Query
	{
		public Expression<Func<TEntity, bool>> ExpressionFilters { get; set; }

		public override Query Parse(IWebContext webContext, bool isCollectionCall = true)
		{
			var query = base.Parse(webContext, isCollectionCall);

			SetIncludesFromFields(isCollectionCall);

			return query;
		}
		
		private void SetIncludesFromFields(bool isCollectionCall)
		{
			RecursiveInclude(Fields, typeof(TEntity), new string[] { });
		}

		private void RecursiveInclude(Field field, Type entityType, string[] propertyTree)
		{
			var propertyPath = "";
			if (propertyTree.Any())
			{
				propertyPath = String.Join(".", propertyTree) + ".";
			}

			foreach (var key in field.Keys)
			{
				var property = GetPropertyFromKey(entityType, key);

				if (property != null)
				{
					if (IsIncludeCandidate(property))
					{
						var propertyType = property.PropertyType.GetListOrArrayElementType(); //ICollection<LegalEntity> => LegalEntity

						//On regarde si y'a pas des subs qui sont eux mêmes des EntityBase
						if (field[key].subs.Keys.Any(sub =>
						{
							var subProperty = GetPropertyFromKey(propertyType, sub);
							if (subProperty != null)
							{
								return IsIncludeCandidate(subProperty);
							}
							return false;
						}))
						{
							RecursiveInclude(field[key], propertyType, propertyTree.Concat(new string[] { property.Name }).ToArray());
						}
						else //Si on est au bout d'une branche, on joue le Include
						{
							IncludeProperty(propertyPath, property);
						}
					}
				}
			}
		}
		private PropertyInfo GetPropertyFromKey(Type type, string key)
		{
			return type.GetProperties().Where(p => p.Name.ToLower() == key).FirstOrDefault();
		}
		protected virtual void IncludeProperty(string propertyPath, PropertyInfo property)
		{
			Includes.Add(propertyPath + property.Name);
		}
		protected virtual bool IsIncludeCandidate(PropertyInfo property)
		{
			return property.PropertyType.IsSubclassOfInterface(typeof(IIncludable))
						|| (property.PropertyType.IsGenericType && property.PropertyType.GetGenericArguments()[0].IsSubclassOfInterface(typeof(IIncludable)));
		}
	}
}
