using RDD.Infra.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Helpers
{
	public class PostedDataHelper
	{
		public object TryConvert(PostedData data, Type declaringType, string propertyName)
		{
			//Si le type déclarant la propriété est une List ou un Array, on s'intéresse au type contenu dedans
			declaringType = declaringType.GetListOrArrayElementType();

			var elements = propertyName.Split('.');

			//Si on demande une propriété directe de T, alors on peut répondre
			if (elements.Count() == 1)
			{
				//On n'utilise pas les jsonProperties (ex appName dans api/EmailAttachments, mode dans api/leaves ...)
				Type propertyType;
				Type conversionType;
				var typeName = declaringType.GetRealTypeName();
				var properties = declaringType.GetPublicProperties();
				var propertyKeys = properties.Select(p => p.Name);

				if (propertyName != "this")
				{
					var property = properties.Where(p => p.Name.ToLower() == propertyName).FirstOrDefault();
					propertyType = property.PropertyType;

					conversionType = propertyType;
					//if (property.isNullable)//Les ED
					//{
					//    conversionType = propertyType.GetNullableType();
					//}
				}
				else //C'est l'objet lui-même
				{
					propertyType = declaringType;
					conversionType = propertyType;
				}

				//Si la propriété est une List<T> ou T[]
				if (propertyType.IsListOrArray())
				{
					var elementsType = propertyType.GetListOrArrayElementType();

					//Soit on a dans data.value l'URL API de la collection d'éléments => "/api/users?id=123,124"
					if (!String.IsNullOrEmpty(data.value))
					{
						return TryConvert(data.value, conversionType);
					}
					else //Ici on considère qu'on a tableau JSON d'éléments => "[ "/api/users/123", "/api/users/124" ]"
					{
						return null;// this.CastEnumerableIntoStrongType(propertyType, data.subs.Values.Select(s => TryConvert(s, elementsType, "this", appInstance, BackDoorGet)).ToList());
					}
				}
				else
				{
					if (data.HasSubs)
					{
						var complexObject = Activator.CreateInstance(propertyType);
						foreach (var key in data.subs.Keys)
						{
							var objectProperty = propertyType.GetProperty(key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase);

							if (objectProperty != null)
							{
								objectProperty.GetSetMethod().Invoke(complexObject, new object[] { TryConvert(data.subs[key], conversionType, key) });
							}
						}
						return complexObject;
					}
					else
					{
						return TryConvert(data.value, conversionType);
					}
				}
			}
			else //Ici on doit relancer la méthode avec le type suivant
			{
				var baseElement = elements[0];
				var subElements = elements.Skip(1);

				var baseProperty = declaringType.GetProperties().Where(p => p.Name.ToLower() == baseElement).FirstOrDefault();

				if (baseProperty != null)
				{
					return TryConvert(data, baseProperty.PropertyType, String.Join(".", subElements.ToArray()));
				}
				else
				{
					throw new Exception(String.Format("Property {0} does not exist on type {1}", baseElement, declaringType.Name));
				}
			}
		}

		private object TryConvert(string stringValue, Type propertyType)
		{
			var realStringValue = stringValue;

			//Cas particulier pour les mots clés
			switch (stringValue)
			{
				case "today":
					realStringValue = DateTime.Today.ToISO();
					break;

				default:
					break;
			}


			//Cas particulier pour les URL
			//var appURL = AliasApplication.Current.rootUri;
			//if (!String.IsNullOrEmpty(appURL) && !String.IsNullOrEmpty(realStringValue) && (realStringValue.StartsWith(appURL) || realStringValue.StartsWith("/api/")))
			//{
			//    Uri uriValue;
			//    if (Uri.TryCreate(realStringValue, UriKind.RelativeOrAbsolute, out uriValue))
			//    {
			//        var rawAPIObject = BackDoorGet(uriValue.ToString());

			//        if (propertyType.IsListOrArray())
			//        {
			//            return this.CastEnumerableIntoStrongType(propertyType, (IEnumerable<object>)rawAPIObject);
			//        }
			//        else
			//        {
			//            return rawAPIObject;
			//        }
			//    }
			//    else
			//    {
			//        throw new Exception(String.Format("Malformed or bad internal api uri {0}", stringValue));
			//    }
			//}
			//else
			//{
			var nullableType = Nullable.GetUnderlyingType(propertyType);

			try
			{
				if ((nullableType != null || !propertyType.IsValueType) && (String.IsNullOrEmpty(realStringValue) || realStringValue == "null"))
				{
					if (realStringValue == String.Empty)
					{
						return String.Empty;
					}
					else
					{
						return null;
					}
				}
				else
				{
					var correctPropertyType = nullableType ?? propertyType;
					var culture = CultureInfo.CurrentCulture;

					//On permet, qq soit la culture du user, d'envoyer des doubles avec . ou , comme séparateur décimal
					if (correctPropertyType == typeof(Double) || correctPropertyType == typeof(Decimal))
					{
						realStringValue = realStringValue.Replace(",", ".");
						culture = CultureInfo.InvariantCulture;
					}
					if (correctPropertyType == typeof(DateTime))
					{
						culture = CultureInfo.InvariantCulture;
					}


					return realStringValue.ChangeType(correctPropertyType, culture);
				}
			}
			catch
			{
				throw new Exception(String.Format("String {0} is not compatible with type {1}", realStringValue, propertyType.Name));
			}
			//}
		}

		/// <summary>
		/// Permet de convertir un List of Utilisateur_SPOREOIREUR en APICollection of Utilisateur tout court
		/// </summary>
		/// <param name="propertyType"></param>
		/// <param name="elements"></param>
		/// <returns></returns>
		public object CastEnumerableIntoStrongType(Type propertyType, IEnumerable<object> elements)
		{
			//ON part d'une List<T> fortement typée
			var listConstructorParamType = typeof(List<>).MakeGenericType(propertyType.GetListOrArrayElementType());

			var properTypeParamList = (IList)Activator.CreateInstance(listConstructorParamType);

			foreach (var element in elements)
			{
				properTypeParamList.Add(element);
			}

			if (propertyType.IsArray)
			{
				return ((dynamic) properTypeParamList).ToArray();
			}

			var genericTypeDefinition = propertyType.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(IEnumerable<>))
			{
				return properTypeParamList;
			}
			if (genericTypeDefinition == typeof(ICollection<>))
			{
				return properTypeParamList;
			}
			if (genericTypeDefinition == typeof(HashSet<>))
			{
				return properTypeParamList;
			}
			if (genericTypeDefinition == typeof(List<>))
			{
				return properTypeParamList;
			}
			if (genericTypeDefinition == typeof(RestCollection<,>))
			{
				var apiCollectionConstructor = propertyType.GetConstructor(new Type[] { });

				return apiCollectionConstructor.Invoke(new object[] { });
			}
			throw new Exception(String.Format("Unhandled enumerable type {0}", propertyType.Name));
		}
	}
}