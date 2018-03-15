using Microsoft.AspNetCore.Http;
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

        public EntitySerializer(IUrlProvider urlProvider)
        {
            _defaultSerializer = new PropertySerializer(this, urlProvider);
            _mappings = new Dictionary<Type, PropertySerializer>();

            Map<Culture, CultureSerializer>(s => new CultureSerializer(s, urlProvider));
        }

        protected void Map<TEntity, TSerializer>(Func<IEntitySerializer, TSerializer> Initiator)
            where TSerializer : PropertySerializer
        {
            var serializer = Initiator(this);
            _mappings.Add(typeof(TEntity), serializer);
        }

        public PropertySerializer GetPropertySerializer(Type TEntity)
        {
            var key = _mappings.Keys.FirstOrDefault(k => k.IsAssignableFrom(TEntity));
            return key != null ? _mappings[key] : _defaultSerializer;
        }

        public Dictionary<string, object> SerializeSelection<TEntity>(ISelection<TEntity> collection, Query<TEntity> query)
            where TEntity : class
        {
            var result = new Dictionary<string, object>();

            foreach (var collectionField in query.CollectionFields)
            {
                var child = collectionField.EntitySelector;
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

        public Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, Field field)
        {
            return SerializeEntity(entity, new HashSet<Field> { field });
        }
        public Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, IEnumerable<Field> fields)
        {
            return SerializeEntities(new List<TEntity> { entity }, fields).FirstOrDefault();
        }
        public Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, PropertySelector field)
        {
            return SerializeEntities(new List<TEntity> { entity }, new HashSet<PropertySelector> { field }).FirstOrDefault();
        }
        public Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, IEnumerable<PropertySelector> fields)
        {
            return SerializeEntities(new List<TEntity> { entity }, fields).FirstOrDefault();
        }
        public List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, IEnumerable<Field> fields)
        {
            return SerializeEntities(entities, fields.Select(f => f.EntitySelector));
        }
        public List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, PropertySelector field)
        {
            return SerializeEntities(entities, new HashSet<PropertySelector> { field });
        }
        public virtual List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, IEnumerable<PropertySelector> fields)
        {
            if (entities.Any())
            {
                var result = new List<Dictionary<string, object>>();
                foreach (var entity in entities)
                {
                    var entityToDictionary = new Dictionary<string, object>();
                    if (entity != null)
                    {
                        var propertySerializer = GetPropertySerializer(entity.GetType());

                        //Si c'est un entitybase mais qu'on ne demande aucun field particulier, on va renvoyer id, name, url
                        if (!fields.Any())
                        {
                            var entityType = entity.GetType();
                            var parser = new FieldsParser();

                            if (entityType.IsSubclassOfInterface(typeof(IEntityBase)))
                            {
                                fields = parser.ParseFields<TEntity>("id, name, url").Select(f => f.EntitySelector);
                            }
                            else
                            {
                                fields = parser.ParseAllProperties(entityType).Select(f => f.EntitySelector);
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

        public Dictionary<string, object> SerializeException<TException>(TException e)
            where TException : Exception, IStatusCodeException
        {
            return new Dictionary<string, object>()
            {
                { "Status", e.StatusCode},
                { "Message", e.Message},
                { "Data", e.Data}
            };
        }

        public Dictionary<string, object> SerializeExceptionWithStackTrace<TException>(TException e)
            where TException : Exception, IStatusCodeException
        {
            return new Dictionary<string, object>()
            {
                { "Status", e.StatusCode},
                { "Message", e.Message},
                { "Data", e.Data},
                { "StackTrace", e.StackTrace}
            };
        }
    }
}