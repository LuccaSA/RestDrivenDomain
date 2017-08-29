using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections;
using RDD.Domain.Models.Querying;
using NExtends.Primitives;
using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using RDD.Domain;

namespace RDD.Domain.Helpers
{
	public class PatchEntityHelper
	{
//		protected IRepoProvider _provider;
		protected IStorageService _storage;

		public PatchEntityHelper(IStorageService storage)
		{
			_storage = storage;
		}

		/// <summary>
		/// Utilisé pour setter les props d'une entité lors d'un POST ou un PUT
		/// Et récursivement pour les sous entités
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="datas"></param>
		public void PatchEntity(object entity, PostedData datas)
		{
			var entityType = Nullable.GetUnderlyingType(entity.GetType()) ?? entity.GetType();
			var props = entityType.GetProperties();

			//On modifie propriété par propriété
			foreach (var key in datas.Keys)
			{
				var property = props.FirstOrDefault(p => p.Name.ToLower() == key);

				if (property == null)
				{
					throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("Property {0} does not exist on type {1}", key, entityType.Name));
				}

				//Si la propriété n'est pas publique, alors on indique qu'on ne peut pas la modifier
				var propertySetter = property.GetSetMethod();

				if (propertySetter == null)
				{
					throw new HttpLikeException(HttpStatusCode.Forbidden, String.Format("Property {0} of type {1} is not writable", property.Name, entityType.Name));
				}

				var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
				var subType = propertyType.GetEnumerableOrArrayElementType();

				//Si c'est un type du domaine, on doit aller chercher les entités via leur repo, pour garantir qu'ils seront récupérés via le contexte courant (et pas créé au moment du commit)
				if (typeof(IEntityBase).IsAssignableFrom(subType))
				{
					var entityBaseType = subType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEntityBase<,>));
					if (entityBaseType != null)
					{
						//Collection de Type du domaine (1-N ou N-N)
						if (propertyType.IsGenericType)
						{
							// Si on envoi un tableau d'objets non vides, on récupère ces objets puis on les affecte à la propriété
							// Le else est utilisé pour gérer l'écrasement lorsqu'on envoi un tableau vide
							if (datas[key].subs.Values.Any())
							{
								var entitiesGetter = GetSimpleEntitiesGetter(datas[key].subs.Values.First(), subType, entityBaseType);

								var attachedEntities = entitiesGetter(datas[key]);
								var values = new SerializationService().CastEnumerableIntoStrongType(property.PropertyType, attachedEntities);
								propertySetter.Invoke(entity, new[] { values });
							}
							else
							{
								var values = new SerializationService().CastEnumerableIntoStrongType(property.PropertyType, new HashSet<object>());
								propertySetter.Invoke(entity, new[] { values });
							}
						}
						else //Type du domaine (N-1)
						{
							var entityGetter = GetSimpleEntityGetter(datas[key], subType, entityBaseType);

							var attachedEntity = entityGetter(datas[key]);
							propertySetter.Invoke(entity, new[] { attachedEntity });
						}
					}
				}
				else if (
					(
						propertyType.IsGenericType
						&& propertyType.GetInterface("IEnumerable") != null // Il ne suffit pas d'être générique pour être une collection (eg EnumClient<UneEnum>)
						&& propertyType.GetGenericTypeDefinition() != typeof(Dictionary<,>) // Un dictionnaire est un objet d'objets JSON et non un tableau d'objets
					)
					|| propertyType.IsArray // Les types [] ne sont pas des Generic mais doivent être traités de la même manière
				) //Collection d'objets qui ne sont pas du Domaine (ou complexType)
				{
					var jsonString = String.Join(",", datas[key].subs.Values.Select(v => v.rawObject != null ? v.rawObject.ToString() : (v.value != null ? v.value.ToString() : "null")));

					if (property.PropertyType.IsEnumerableOrArray())
					{
						jsonString = "[" + jsonString + "]";
					}
					var propValue = JsonConvert.DeserializeObject(jsonString, property.PropertyType);
					propertySetter.Invoke(entity, new[] { propValue });
				}
				else if (datas[key].HasSubs) //Type non générique mais potentiellement complexType
				{
					//On récupère le sous objet
					var subEntity = property.GetValue(entity);

					//S'il est NULL, il faut l'instancier
					if (subEntity == null)
					{
						subEntity = Activator.CreateInstance(propertyType);
					}

					// Si c'est un dictionnaire on patch différemment
					// Il faut associer les valeurs à une clé et non à une propriété de l'objet
					if (propertyType.IsSubclassOfInterface(typeof(IDictionary)))
					{
						var dictionary = subEntity as IDictionary;

						// Si la classe hérite de Dictionary on prend le base type
						if (!propertyType.IsGenericType)
						{
							propertyType = propertyType.BaseType;
						}
						// On récupère les génériques
						var genericArguments = propertyType.GetGenericArguments();

						// On construit l'objet
						foreach (var sub in datas[key].subs)
						{
							// Une clé JSON est forcément une string - voir http://json.org/
							// En revanche côté serveur il peut être utile d'avoir des dictionnaires de int,
							// c'est pourquoi on convertit la clé vers genericArguments[0] au lieu de string
							var dicKey = sub.Key.ChangeType(genericArguments[0], CultureInfo.InvariantCulture);
							var dicValue = sub.Value.rawObject != null ? JsonConvert.DeserializeObject(sub.Value.rawObject.ToString(), genericArguments[1]) : null;

							// Si c'est un type valeur
							if (dicValue == null && genericArguments[1].IsValueType)
							{
								if (sub.Value.value != null)
								{
									// On ne peut pas convertir un String en Nullable<int>
									// On récupère donc le générique (par exemple int)
									if (genericArguments[1].IsGenericType)
									{
										dicValue = sub.Value.value.ChangeType(genericArguments[1].GenericTypeArguments[0], CultureInfo.InvariantCulture);
									}
									else
									{
										dicValue = sub.Value.value.ChangeType(genericArguments[1], CultureInfo.InvariantCulture);
									}
								}
								// Il est impossible de setter à NULL une propriété non nullable
								else if (!genericArguments[1].IsTypeNullable())
								{
									throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("You cannot set a non nullable value to NULL (Property {0})", key));
								}
							}

							dictionary[dicKey] = dicValue;

						}

						// Enfin, on patch
						propertySetter.Invoke(entity, new[] { dictionary });
					}
					else
					{
						PatchEntity(subEntity, datas[key]);

						propertySetter.Invoke(entity, new[] { subEntity });
					}
				}
				else //Type simple, on envoie la valeur
				{
					propertySetter.Invoke(entity, new[] { datas[key].value == null ? null : datas[key].value.ChangeType(propertyType, CultureInfo.InvariantCulture) });
				}
			}
		}

		/// <summary>
		/// Permet d'aller chercher 1 sous entité dans le cadre d'une relation N-1 par exemple
		/// </summary>
		/// <param name="entityData"></param>
		/// <param name="subType"></param>
		/// <param name="entityBaseType"></param>
		/// <returns></returns>
		protected virtual Func<PostedData, object> GetSimpleEntityGetter(PostedData entityData, Type subType, Type entityBaseType)
		{
			//TODO
			//if (entityData.ContainsKey("id"))
			//{
			//	var keyType = entityBaseType.GetGenericArguments()[1];
			//	var repo = _provider.TryGetRepository(_storage, subType, keyType, null);
			//	return (s) => repo.TryGetById(Convert.ChangeType(s["id"].value, keyType));
			//}
			//else
			//{
				return (s) => s.rawObject.ToObject(subType);
//			}
		}

		/// <summary>
		/// Permet d'aller chercher N entités d'un coup dans le cadre d'une relation 1-N
		/// </summary>
		/// <param name="firstOfArray"></param>
		/// <param name="subType"></param>
		/// <param name="entityBaseType"></param>
		/// <returns></returns>
		protected virtual Func<PostedData, IEnumerable<object>> GetSimpleEntitiesGetter(PostedData firstOfArray, Type subType, Type entityBaseType)
		{
			//TODO
			//if (firstOfArray.ContainsKey("id"))
			//{
			//	var keyType = entityBaseType.GetGenericArguments()[1];
			//	var repo = _provider.TryGetRepository(_context, subType, keyType, null);
			//	return (s) => repo.TryGetByIds(s.subs.Values.Select(el => Convert.ChangeType(el["id"].value, keyType)));
			//}
			//else
			//{
				return (s) => s.subs.Values.Select(el => el.rawObject.ToObject(subType));
		//	}
		}
	}
}