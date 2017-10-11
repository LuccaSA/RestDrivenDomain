using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Querying;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Serialization
{
    public class EntitySerializer : IEntitySerializer
    {
        private readonly Dictionary<Type, PropertySerializer> _mappings;
        private readonly PropertySerializer _defaultSerializer;
        protected PluralizationService _pluralizationService;

        public EntitySerializer()
        {
            _pluralizationService = new PluralizationService();
            _defaultSerializer = new PropertySerializer(this);
            _mappings = new Dictionary<Type, PropertySerializer>();

            Map<Culture, CultureSerializer>((s) => new CultureSerializer(s));
        }

        protected void Map<TEntity, TSerializer>(Func<IEntitySerializer, TSerializer> Initiator)
            where TSerializer : PropertySerializer, new()
        {
            var serializer = Initiator(this);
            _mappings.Add(typeof(TEntity), serializer);
        }

        public PropertySerializer GetPropertySerializer(Type TEntity)
        {
            var key = _mappings.Keys.FirstOrDefault(k => k.IsAssignableFrom(TEntity));
            return key != null ? _mappings[key] : _defaultSerializer;
        }

        public virtual string GetUrlTemplateFromEntityType(Type entityType)
        {
            var apiRadical = _pluralizationService.GetPlural(entityType.Name).ToLower();

            return String.Format("api/v3/{0}/{{0}}", apiRadical);
        }

        public Dictionary<string, object> SerializeSelection<TEntity>(ISelection<TEntity> collection, Query<TEntity> query)
            where TEntity : class, IEntityBase
        {
            var result = new Dictionary<string, object>();

            foreach (var child in query.CollectionFields.EntitySelector.Children)
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

                    sums.Add(child.Subject, value); //Sum(..)
                }
                else if (childName.ToLower() == "min")
                {
                    Dictionary<string, object> mins;
                    if (result.ContainsKey("mins"))
                    {
                        mins = (Dictionary<string, object>)result["mins"];
                    }
                    else
                    {
                        mins = new Dictionary<string, object>();
                        result.Add("mins", mins);
                    }

                    mins.Add(child.Subject, value); //Min(..)
                }
                else if (childName.ToLower() == "max")
                {
                    Dictionary<string, object> maxes;
                    if (result.ContainsKey("maxes"))
                    {
                        maxes = (Dictionary<string, object>)result["maxes"];
                    }
                    else
                    {
                        maxes = new Dictionary<string, object>();
                        result.Add("maxes", maxes);
                    }

                    maxes.Add(child.Subject, value); //Max(..)
                }
                else
                {
                    result.Add(child.Name, value); //.Count, ...
                }
            }

            result.Add("items", SerializeEntities(collection.Items, query.Fields));

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
                                fields = new FieldsParser().ParseAllProperties<TEntity>().EntitySelector;
                            }
                        }

                        entityToDictionary = propertySerializer.SerializeProperties(entity, fields);
                    }

                    result.Add(entityToDictionary);
                }
                return result;
            }

            return new List<Dictionary<string, object>>();
        }

        public Dictionary<string, object> SerializeException(HttpLikeException e)
        {
            return new Dictionary<string, object>()
            {
                { "Status", e.Status},
                { "Message", e.Message},
                { "Data", e.Data}
            };
        }

        public Dictionary<string, object> SerializeExceptionWithStackTrace(HttpLikeException e)
        {
            return new Dictionary<string, object>()
            {
                { "Status", e.Status},
                { "Message", e.Message},
                { "Data", e.Data},
                { "StackTrace", e.StackTrace}
            };
        }
    }
}