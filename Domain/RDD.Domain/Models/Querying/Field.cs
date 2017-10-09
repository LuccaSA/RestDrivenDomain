using System;
using System.Linq;
using System.ComponentModel;
using System.Linq.Expressions;
using RDD.Domain.Helpers;

namespace RDD.Domain.Models.Querying
{
	public class Field
	{
		public enum Reserved
		{
			[Description("Champ permettant de demander les actions possibles sur les entités")]
			actions,
			[Description("Champ permettant de demander des propriétés de la collection elle-même plutôt que des propriétés des entités")]
			collection,
			[Description("Champ permettant de demander les operations possibles sur les entités")]
			operations
		}

		protected Type EntityType { get; set; }
		public PropertySelector EntitySelector { get; protected set; }

		//public static Field<TEntity> New<TEntity>()
		//{
		//	return new Field<TEntity>();
		//}
		//public static Field NewFromType(Type entityType)
		//{
		//	return (Field)typeof(Field).GetMethod("New").MakeGenericMethod(entityType).Invoke(null, new object[] { });
		//}

		public bool HasChild => EntitySelector.HasChild;
	    public bool IsEmpty => !HasChild;
	    public int Count => EntitySelector.Count;
	}

	public class Field<TEntity> : Field
	{
		public Field()
		{
			EntityType = typeof(TEntity);
			EntitySelector = new PropertySelector<TEntity>();
		}

		public Field(params Expression<Func<TEntity, object>>[] expressions)
			: this()
		{
			Add(expressions);
		}

		public bool Add(Expression<Func<TEntity, object>> field)
		{
			return EntitySelector.Add(field);
		}

		public bool Add(params Expression<Func<TEntity, object>>[] expressions)
		{
			return expressions.Select(expression =>
			{
				return Add(expression);
			}).Aggregate((b1, b2) => b1 && b2);
		}

		public bool Contains(Expression<Func<TEntity, object>> expression)
		{
			return EntitySelector.Contains(expression);
		}

		public bool ContainsEmpty(Expression<Func<TEntity, object>> expression)
		{
			return EntitySelector.ContainsEmpty(expression);
		}

		public bool ContainsAny(params Expression<Func<TEntity, object>>[] expressions)
		{
			return ((PropertySelector<TEntity>)EntitySelector).ContainsAny(expressions);
		}

		/// <summary>
		/// Permet de transférer un selecteur vers un enfant
		/// NB : utiliser p. comme paramètre du selecteur
		/// </summary>
		/// <typeparam name="TSub"></typeparam>
		/// <param name="selector"></param>
		/// <returns></returns>
		public Field<TSub> TransfertTo<TSub>(LambdaExpression selector)
		{
			var child = new Field<TSub>();
			child.EntitySelector = ((PropertySelector<TEntity>)EntitySelector).TransfertTo<TSub>(selector);

			return child;
		}

		public Field<TBase> Cast<TBase>()
		{
			var result = new Field<TBase>();

			result.EntitySelector = ((PropertySelector<TEntity>)EntitySelector).Cast<TBase>();

			return result;
		}
	}
}