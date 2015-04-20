using Newtonsoft.Json;
using RDD.Infra.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
    public static class TypeExtensions
    {
		public static bool IsSubclassOfInterface(this Type toCheck, Type interfaceType)
		{
			var interfaces = toCheck.GetInterfaces();

			foreach (var interfaceOfCheck in interfaces)
			{
				if (interfaceOfCheck.IsGenericType)
				{
					if (interfaceOfCheck.GetGenericTypeDefinition() == interfaceType)
					{
						return true;
					}
				}
			}

			return false;
		}
		public static Type GetListOrArrayElementType(this Type T)
		{
			if (T.IsArray)
			{
				return T.GetElementType();
			}
			else if (T.IsListOrArray())
			{
				return T.GetGenericArguments().FirstOrDefault();
			}
			else
			{
				return T;
			}
		}
		public static string GetRealTypeName(this Type type)
		{
			if (!Attribute.IsDefined(type, typeof(JsonObjectAttribute), false))
			{
				return (type.BaseType ?? type).Name; //In case BaseType is null, we take the Type itself (Object for example)
			}
			else
			{
				return type.Name.Split('`')[0];
			}
		}
		/// <summary>
		/// Permet d'aller chercher les propriétés hérités pour les Interfaces => IUser : IEntityBase
		/// http://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetPublicProperties(this Type type)
		{
			if (type.IsInterface)
			{
				var propertyInfos = new List<PropertyInfo>();

				var considered = new List<Type>();
				var queue = new Queue<Type>();
				considered.Add(type);
				queue.Enqueue(type);
				while (queue.Count > 0)
				{
					var subType = queue.Dequeue();
					foreach (var subInterface in subType.GetInterfaces())
					{
						if (considered.Contains(subInterface)) continue;

						considered.Add(subInterface);
						queue.Enqueue(subInterface);
					}

					var typeProperties = subType.GetProperties(
						BindingFlags.FlattenHierarchy
						| BindingFlags.Public
						| BindingFlags.Instance);

					var newPropertyInfos = typeProperties
						.Where(x => !propertyInfos.Contains(x));

					propertyInfos.InsertRange(0, newPropertyInfos);
				}

				return propertyInfos.ToArray();
			}

			return type.GetProperties(BindingFlags.FlattenHierarchy
				| BindingFlags.Public | BindingFlags.Instance);
		}
		public static bool IsListOrArray(this Type T)
		{
			return (T.IsGenericType && (T.GetGenericTypeDefinition() == typeof(List<>) || T.GetGenericTypeDefinition() == typeof(HashSet<>) || T.GetGenericTypeDefinition() == typeof(ICollection<>) || T.GetGenericTypeDefinition() == typeof(IEnumerable<>) || T.GetGenericTypeDefinition() == typeof(RestCollection<,>))) || T.IsArray;
		}
		public static bool IsEnumerableOrArray(this Type T)
		{
			return (T.IsGenericType && (T.GetGenericTypeDefinition() == typeof(IEnumerable<>)
				|| T.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsEnumerableOrArray()))
				)
					 || T.IsArray;
		}
		public static Type GetEnumerableOrArrayElementType(this Type T)
		{
			if (!IsEnumerableOrArray(T))
			{
				return T;
			}
			// On est sûr que c'est un type Enumerable<> ou un Array
			// Donc si c'est pas un Array, on retourn le type generique
			if (T.IsArray)
			{
				return T.GetElementType();
			}
			else
			{
				return T.GetGenericArguments().FirstOrDefault();
			}
		}
		public static object CastEnumerableIntoStrongType(Type propertyType, IEnumerable<object> elements)
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
				return ((dynamic)properTypeParamList).ToArray();
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
