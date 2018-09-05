using RDD.Domain.Json;
using System;
using System.Reflection;

namespace RDD.Domain.Patchers
{
    class DynamicPatcher : IPatcher
	{
        object IPatcher.InitialValue(PropertyInfo property, object patchedObject)
        {
            return null;
        }

		object IPatcher.PatchValue(object patchedObject, Type expectedType, IJsonElement json)
		{
			return PatchValue(patchedObject, expectedType, json as JsonObject);
		}

		public virtual object PatchValue(object patchedObject, Type expectedType, JsonObject json)
		{
			if (json == null)
				return null;

			return json.GetContent();
		}
	}
}