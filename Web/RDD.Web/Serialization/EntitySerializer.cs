using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Linq.Expressions;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Helpers;
using RDD.Domain;
using NExtends.Primitives;

namespace RDD.Web.Serialization
{
	public abstract class EntitySerializer : IEntitySerializer
	{
		private readonly Dictionary<Type, PropertySerializer> _mappings;
		private readonly PropertySerializer _defaultSerializer;
		protected PluralizationCacheService _pluralizationCacheService;

		protected EntitySerializer()
		{
			_pluralizationCacheService = new PluralizationCacheService();
			_defaultSerializer = new PropertySerializer(this);
			_mappings = new Dictionary<Type, PropertySerializer>();
		}

		protected void Map<TEntity, TSerializer>(Func<IEntitySerializer, PluralizationCacheService, TSerializer> Initiator)
			where TSerializer : PropertySerializer, new()
		{
			var serializer = Initiator(this, _pluralizationCacheService);
			_mappings.Add(typeof(TEntity), serializer);
		}

		public PropertySerializer GetPropertySerializer(Type TEntity)
		{
			var key = _mappings.Keys.FirstOrDefault(k => k.IsAssignableFrom(TEntity));
			return key != null ? _mappings[key] : _defaultSerializer;
		}

		public virtual string GetUrlTemplateFromEntityType(Type entityType)
		{
			var apiRadical = _pluralizationCacheService.GetPlural(entityType.Name).ToLower();

			return String.Format("api/v3/{0}/{{0}}", apiRadical);
		}

		public Dictionary<string, object> SerializeCollection<TEntity>(IRestCollection<TEntity> collection, Field<TEntity> fields)
			where TEntity : class, IEntityBase
		{
			var result = new Dictionary<string, object>();

			foreach (var child in fields.CollectionSelector.Children)
			{
				var childName = child.Name;
				var value = child.Lambda.Compile().DynamicInvoke(collection);

				if (childName.ToLower() == "sum")
				{
					Dictionary<string, object> sums;
					if (result.ContainsKey("sums"))
					{
						sums = (Dictionary<string, object>)result["sums"];
					}
					else
					{
						sums = new Dictionary<string, object>();
						result.Add("sums", sums);
					}
					if (!result.ContainsKey("sums"))
					{
						result.Add("sums", new Dictionary<string, object>());
					}

					sums.Add(child.Subject, value); //Sum(..)
				}
				else
				{
					result.Add(child.Name, value); //.Count, ...
				}
			}

			result.Add("items", SerializeEntities(collection.Items, fields.EntitySelector));

			return result;
		}

		public Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, Field<TEntity> fields)
		{
			return fields == null ? new Dictionary<string, object>() : SerializeEntity(entity, fields.EntitySelector);
		}
		public virtual Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, PropertySelector fields)
		{
			return SerializeEntities(new List<TEntity> { entity }, fields).FirstOrDefault();
		}
		public List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, Field<TEntity> fields)
		{
			return fields == null ? new List<Dictionary<string, object>>() : SerializeEntities(entities, fields.EntitySelector);
		}
		public List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, PropertySelector fields)
		{
			if (fields != null && entities.Any())
			{
				var result = new List<Dictionary<string, object>>();
				foreach (var entity in entities)
				{
					var entityToDictionary = new Dictionary<string, object>();
					if (entity != null)
					{
						var propertySerializer = GetPropertySerializer(entity.GetType());

						//Si c'est un entitybase mais qu'on ne demande aucun field particulier, on va renvoyer id, name, url
						if (!fields.HasChild)
						{
							if (entity.GetType().IsSubclassOfInterface(typeof(IEntityBase)))
							{
								fields.Parse("id");
								fields.Parse("name");
								fields.Parse("url");
							}
							else
							{
								fields = FieldHelper.ParseAllProperties(entity.GetType());
							}
						}

						foreach (var child in fields.Children)
						{
							entityToDictionary.Add(child.GetCurrentProperty().Name, propertySerializer.SerializeProperty(entity, child));
						}
					}
					result.Add(entityToDictionary);
				}
				return result;
			}

			return new List<Dictionary<string, object>>();
		}
	}
}