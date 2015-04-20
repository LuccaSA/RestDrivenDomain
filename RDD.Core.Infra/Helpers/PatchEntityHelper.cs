using Newtonsoft.Json;
using RDD.Infra;
using RDD.Infra.Models.Exceptions;
using RDD.Infra.Models.Querying;
using RDD.Infra.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Helpers
{
	public class PatchEntityHelper
	{
		protected IStorageService _storage;
		protected IExecutionContext _execution;

		public PatchEntityHelper(IStorageService storage, IExecutionContext execution)
		{
			_storage = storage;
			_execution = execution;
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
				var subType = propertyType.GetListOrArrayElementType();

				//Si c'est un type du domaine, on doit aller chercher les entités via leur repo, pour garantir qu'ils seront récupérés via le contexte courant (et pas créé au moment du commit)
				if (typeof(IEntityBase).IsAssignableFrom(subType))
				{
					var entityBaseType = subType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEntityBase<>));
					if (entityBaseType != null)
					{
						Type keyType;
						IRestService service;
						Func<PostedData, object> entityGetter;

						//Collection de Type du domaine (1-N ou N-N)
						if (propertyType.IsGenericType)
						{
							if (datas[key].subs.Values.Any())
							{
								if (datas[key].subs.Values.First().ContainsKey("id"))
								{
									keyType = entityBaseType.GetGenericArguments()[1];
									service = RestServiceProvider.TryGetRepository(subType, keyType, _storage, _execution, null);
									entityGetter = (s) => service.TryGetById(Convert.ChangeType(s["id"].value, keyType));
								}
								else
								{
									entityGetter = (s) => s.rawObject.ToObject(subType);
								}

								var attachedEntities = new List<object>(datas[key].subs.Values.Select(entityGetter));
								var values = TypeExtensions.CastEnumerableIntoStrongType(property.PropertyType, attachedEntities);
								propertySetter.Invoke(entity, new[] { values });
							}
						}
						else //Type du domaine (N-1)
						{
							entityGetter = GetSimpleEntityGetter(datas[key], subType, entityBaseType);

							var attachedEntity = entityGetter(datas[key]);
							propertySetter.Invoke(entity, new[] { attachedEntity });
						}
					}
				}
				else if (propertyType.IsGenericType) //Collection d'objets qui ne sont pas du Domaine (ou complexType)
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

					PatchEntity(subEntity, datas[key]);
				}
				else //Type simple, on envoie la valeur
				{
					propertySetter.Invoke(entity, new[] { datas[key].value == null ? null : datas[key].value.ChangeType(propertyType, CultureInfo.InvariantCulture) });
				}
			}
		}

		protected virtual Func<PostedData, object> GetSimpleEntityGetter(PostedData entityData, Type subType, Type entityBaseType)
		{
			if (entityData.ContainsKey("id"))
			{
				var keyType = entityBaseType.GetGenericArguments()[1];
				var repo = RestServiceProvider.TryGetRepository(subType, keyType, _storage, _execution, null);
				return (s) => repo.TryGetById(Convert.ChangeType(s["id"].value, keyType));
			}
			else
			{
				return (s) => s.rawObject.ToObject(subType);
			}
		}
	}
}
