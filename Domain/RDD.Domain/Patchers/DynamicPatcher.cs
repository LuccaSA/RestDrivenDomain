using Rdd.Domain.Json;
using System;
using System.Reflection;

namespace Rdd.Domain.Patchers
{
    class DynamicPatcher : IPatcher
	{
        object IPatcher.InitialValue(PropertyInfo property, object patchedObject) => null;

        object IPatcher.PatchValue(object patchedObject, Type expectedType, IJsonElement json)
		{
			return PatchValue(patchedObject, expectedType, json as JsonObject);
		}

        public virtual object PatchValue(object patchedObject, Type expectedType, JsonObject json) => json?.GetContent();
    }
}