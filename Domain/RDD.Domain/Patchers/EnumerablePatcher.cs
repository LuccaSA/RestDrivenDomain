using NExtends.Primitives.Types;
using RDD.Domain.Exceptions;
using RDD.Domain.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Domain.Patchers
{
    internal class EnumerablePatcher : IPatcher<JsonArray>
	{
        protected IPatcherProvider Provider { get; set; }

        public EnumerablePatcher(IPatcherProvider provider)
        {
            Provider = provider;
        }

        object IPatcher.InitialValue(PropertyInfo property, object patchedObject)
        {
            return null;
        }

		object IPatcher.PatchValue(object patchedObject, Type expectedType, IJsonElement json)
		{
			return PatchValue(patchedObject, expectedType, json as JsonArray);
		}

		public virtual object PatchValue(object patchedObject, Type expectedType, JsonArray json)
		{
			if (json == null)
				return null;

			var result = new List<object>();
			var elementType = expectedType.GetEnumerableOrArrayElementType();

			foreach (var element in json.Content)
			{
				var patcher = Provider.GetPatcher(elementType, element);
				var value = patcher.PatchValue(null, elementType, element);
				result.Add(value);
			}

			return CastIntoStrongType(expectedType, result);
		}

		//internal for testing
		internal object CastIntoStrongType(Type expectedType, List<object> elements)
		{
			var elementType = expectedType.GetEnumerableOrArrayElementType();

			//ON part d'une List<T> fortement typée
			var stronglyTypedListType = typeof(List<>).MakeGenericType(elementType);
			var listResult = (IList)Activator.CreateInstance(stronglyTypedListType);

			foreach (var element in elements)
			{
				listResult.Add(element);
			}

			if (expectedType.IsArray)
			{
				var arrayResult = Array.CreateInstance(elementType, listResult.Count);
				listResult.CopyTo(arrayResult, 0);
				return arrayResult;
			}

			var genericTypeDefinition = expectedType.GetGenericTypeDefinition();

			if (genericTypeDefinition == typeof(List<>)
				|| genericTypeDefinition == typeof(ICollection<>)
				|| genericTypeDefinition == typeof(IEnumerable<>)
				|| genericTypeDefinition == typeof(IList<>)
				|| genericTypeDefinition == typeof(IReadOnlyList<>)
				|| genericTypeDefinition == typeof(IReadOnlyCollection<>))
			{
				return listResult;
			}

			if (genericTypeDefinition == typeof(HashSet<>)
				|| genericTypeDefinition == typeof(ISet<>))
			{
				var stronglyTypedHashSetType = typeof(HashSet<>).MakeGenericType(elementType);
				return Activator.CreateInstance(stronglyTypedHashSetType, listResult);
			}

			throw new BadRequestException($"Unhandled enumerable type {expectedType.Name}");
		}
	}
}