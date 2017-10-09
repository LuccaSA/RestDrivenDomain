using LinqKit;
using RDD.Domain.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying.Convertors
{
	internal class FiltersConvertor<TEntity>
		where TEntity : class, IEntityBase
	{
		private PredicateService<TEntity> _predicateService;

		internal FiltersConvertor()
		{
			_predicateService = new PredicateService<TEntity>();
		}

		/// <summary>
		/// Fait des AND entre les where dont les différentes valeurs dont combinées via des OR
		/// /api/users?manager.id=2&departement.id=4,5 devient manager.id == 2 AND ( department.id == 4 OR department.id == 5 )
		/// </summary>
		/// <returns></returns>
		internal Expression<Func<TEntity, bool>> Convert(List<Filter<TEntity>> filters)
		{
			var feed = PredicateBuilder.True<TEntity>();
			var type = typeof(TEntity);

			foreach (var filter in filters)
			{
				feed = feed.And(ToEntityExpression(filter, filter.Values).Expand());
			}

			return feed.Expand();
		}

		private Expression<Func<TEntity, bool>> ToEntityExpression(Filter<TEntity> filter, IList values)
		{
			var property = filter.Property.Children.ElementAt(0);

			switch (filter.Operand)
			{
				case FilterOperand.Equals:
					{
						return _predicateService.BuildBinaryExpression(filter.Operand, property.Name, values);
					}
				case FilterOperand.Between:
				case FilterOperand.GreaterThan:
				case FilterOperand.GreaterThanOrEqual:
				case FilterOperand.LessThan:
				case FilterOperand.LessThanOrEqual:
				case FilterOperand.Since:
				case FilterOperand.Until:
					{
						return _predicateService.OrFactory<TEntity, object>(value => _predicateService.BuildBinaryExpression(filter.Operand, property.Name, value), values);
					}
				case FilterOperand.NotEqual:
					{
						return _predicateService.AndFactory<TEntity, object>(value => _predicateService.BuildBinaryExpression(filter.Operand, property.Name, value), values);
					}
				case FilterOperand.Starts:
					{
						return _predicateService.OrFactory<TEntity, string>(value => _predicateService.BuildStartsExpression(property.Name, value), values);
					}
				case FilterOperand.Like:
					{
						return _predicateService.OrFactory<TEntity, string>(value => _predicateService.BuildLikeExpression(property.Name, value), values);
					}
			}

			throw new IndexOutOfRangeException(String.Format("Unhandled filter condition type {0}", filter.Operand));
		}
	}
}
