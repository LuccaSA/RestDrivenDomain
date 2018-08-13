using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RDD.Domain.Exceptions;

namespace RDD.Web.Serialization
{
    public class PropertySerializer
    {
        private readonly IUrlProvider _urlProvider;

        private IEntitySerializer _serializer { get; }

        public PropertySerializer() { throw new NotImplementedException(); }
        public PropertySerializer(IEntitySerializer serializer, IUrlProvider urlProvider)
        {
            _serializer = serializer;
            _urlProvider = urlProvider;
        }

        public virtual Dictionary<string, object> SerializeProperties(object entity, IEnumerable<PropertySelector> fields)
        {
            var result = new Dictionary<string, object>();

            foreach (var field in fields)
            {
                var propertyName = field.GetCurrentProperty().Name;
                var serialized = SerializeProperty(entity, field);

                //Field déjà dans result, il faut aggréger
                if (result.ContainsKey(propertyName))
                {
                    var subsequentProperty = ((Dictionary<string, object>)serialized).FirstOrDefault();
                   
                    MergeDictionaries((Dictionary<string, object>)result[propertyName], subsequentProperty.Key, subsequentProperty.Value);

                    continue;
                }

                result.Add(propertyName, serialized);
            }

            return result;
        }

        private void MergeDictionaries(Dictionary<string, object> target, string key, object value)
        {
            if (target.ContainsKey(key))
            {
                var found = target[key] as Dictionary<string, object>;
                var toMerge = value as Dictionary<string, object>;
                if (found == null || toMerge == null)
                {
                    throw new TechnicalException("Impossible to merge non-dictionaries items");
                }

                foreach (var kv in toMerge)
                {
                    found.Add(kv.Key, kv.Value);
                }
            }
            else
            {
                target.Add(key, value);
            }
        }

        public virtual object SerializeProperty(object entity, PropertySelector field)
        {
            PropertyInfo prop = field.GetCurrentProperty();
            var entityBase = entity as IEntityBase;

            var isUrlFieldOnEntityBase = prop.Name == "Url" && entityBase != null;
            if (isUrlFieldOnEntityBase)
            {
                return _urlProvider.GetEntityUrl(entityBase);
            }

            var value = prop.GetValue(entity, null);

            if (value != null)
            {
                var propertyType = value.GetType();

                //Prop non valeur, on va sérialiser ses sous propriétés
                if (!propertyType.IsValueType() && propertyType != typeof(object))
                {
                    if (propertyType.IsSubclassOfInterface(typeof(IDictionary)))
                    {
                        var dictionary = value as IDictionary;

                        //Si le dictionnaire ne contient aucune entrée
                        //On le sérialise tel quel
                        if (dictionary != null && dictionary.Keys.Count > 0)
                        {

                            //Si c'st un dictionnaire de valeurs simples ou toutes nulles
                            //On le sérialise tel quel
                            var enumerator = dictionary.Values.GetEnumerator();
                            var isNullDictionary = true;
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current != null)
                                {
                                    isNullDictionary = false;
                                    break;
                                }
                            }
                            if (!isNullDictionary && !enumerator.Current.GetType().IsValueType())
                            {
                                //Sinon on va descendre au niveau du type des valeurs pour continuer à les sérialiser
                                var result = new Dictionary<object, object>();

                                foreach (var key in dictionary.Keys)
                                {
                                    result.Add(
                                        key,
                                        dictionary[key] != null ? _serializer.SerializeEntity(dictionary[key], PropertySelector.NewFromType(dictionary[key].GetType(), field.Lambda)) : null
                                    );
                                }

                                value = result;
                            }
                        }
                    }
                    else if (propertyType.IsEnumerableOrArray())
                    {
                        var list = ((IEnumerable)value).Cast<object>();
                        var genericType = propertyType.GetEnumerableOrArrayElementType();

                        //Si c'est une liste d'élément valeur, on sort d'ici, car on la sérialize telle quelle
                        if (!genericType.IsValueType() && genericType != typeof(object))
                        {
                            //Si on est sur une feuille on sérialise les objets en entier
                            if (!field.HasChild)
                            {
                                value = _serializer.SerializeEntities(list, new HashSet<Field>());
                            }
                            else //Sinon on descend sur les enfants
                            {
                                value = _serializer.SerializeEntities(list, field.Child);
                            }
                        }
                    }
                    else
                    {
                        //Si on est sur une feuille on sérialise l'objet en entier
                        if (!field.HasChild)
                        {
                            value = _serializer.SerializeEntity(value, new HashSet<Field>());
                        }
                        else //Sinon on descend sur l'enfant
                        {
                            value = _serializer.SerializeEntity(value, field.Child);
                        }
                    }
                }
            }

            return value;
        }
    }
}