using System;
using System.Linq;
using System.ComponentModel;
using System.Linq.Expressions;
using RDD.Domain.Helpers;
using System.Collections.Generic;

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
        public bool HasChild => EntitySelector.HasChild;
        public bool IsEmpty => !HasChild;

        public Field(Type entityType, PropertySelector selector)
        {
            EntityType = entityType;
            EntitySelector = selector;
        }

        public bool Contains<TEntity>(Expression<Func<TEntity, object>> expression) => EntitySelector.Contains(expression);
    }

    public class Field<TEntity> : Field
    {
        public Field(Expression<Func<TEntity, object>> expression)
            : base(typeof(TEntity), new PropertySelector<TEntity>(expression)) { }
    }
}