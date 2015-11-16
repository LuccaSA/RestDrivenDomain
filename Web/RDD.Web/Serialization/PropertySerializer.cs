using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Linq.Expressions;
using RDD.Domain.Helpers;
using RDD.Domain;
using NExtends.Primitives;

namespace RDD.Web.Serialization
{
	public class PropertySerializer
	{
		private IEntitySerializer _serializer { get; set; }

		public PropertySerializer() { throw new NotImplementedException(); }
		public PropertySerializer(IEntitySerializer serializer)
		{
			_serializer = serializer;
		}

		public virtual object SerializeProperty(object entity, PropertySelector field)
		{
			object value;
			PropertyInfo prop = field.GetCurrentProperty();

			if (prop.Name == "Url")
			{
				var entityType = entity.GetType();
				var typeForUrl = entityType;

				//Ici on suppose que c'est un héritage classique, genre BlogApplication : Application => Application
				// Et on vérifie que entityType n'est pas une EntityBase
				if (!typeof(IEntityBase).IsAssignableFrom(entityType) && entityType.BaseType != null && !entityType.BaseType.IsGenericType)
				{
					typeForUrl = entityType.BaseType;
				}

				value = string.Format(_serializer.GetUrlTemplateFromEntityType(typeForUrl), ((IEntityBase)entity).GetId());
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