using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public virtual Dictionary<string, object> SerializeProperties(object entity, PropertySelector fields)
        {
            var result = new Dictionary<string, object>();

            foreach (var child in fields.Children)
            {
                result.Add(child.GetCurrentProperty().Name, SerializeProperty(entity, child));
            }

            return result;
        }

        public virtual object SerializeProperty(object entity, PropertySelector field)
        {
            object value;
            PropertyInfo prop = field.GetCurrentProperty();
            var entityBase = entity as IEntityBase;

            if (prop.Name == "Url" && entityBase != null)
            {
                value = _urlProvider.GetEntityUrl(entityBase) ;
            }
            else
            {
                value = prop.GetValue(entity, null);

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
                                            dictionary[key] != null ? _serializer.SerializeEntity(dictionary[key], PropertySelector.NewFromType(dictionary[key].GetType())) : null
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

                                value = _serializer.SerializeEntities(list, field);
                            }
                        }
                        else
                        {
                            value = _serializer.SerializeEntity(value, field);
                        }
                    }
                }
            }

            return value;
        }
    }
}