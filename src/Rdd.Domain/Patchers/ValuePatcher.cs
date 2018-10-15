using NExtends.Primitives.Strings;
using NExtends.Primitives.Types;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Json;
using System;
using System.Globalization;
using System.Reflection;

namespace Rdd.Domain.Patchers
{
    public class ValuePatcher : IPatcher
	{
        object IPatcher.InitialValue(PropertyInfo property, object patchedObject)
        {
            return null;
        }

		object IPatcher.PatchValue(object patchedObject, Type expectedType, IJsonElement json)
		{
			return PatchValue(patchedObject, expectedType, json as JsonValue);
		}

		public object PatchValue(object patchedObject, Type expectedType, JsonValue json)
		{
			if (json == null || json.Content == null)
			{
				if (!expectedType.IsTypeNullable())
					throw new BadRequestException($"You cannot set null to a non nullable property");

				return null;
			}

			return json.Content.ToString().ChangeType(expectedType.GetNonNullableType(), CultureInfo.InvariantCulture);
		}
	}
}