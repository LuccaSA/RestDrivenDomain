using RDD.Domain.Json;
using System;

namespace RDD.Domain.Patchers
{
    class EntitiesPatcher : EnumerablePatcher
	{
        public EntitiesPatcher(IPatcherProvider provider)
            : base(provider) { }

        public override object PatchValue(object patchedObject, Type expectedType, JsonArray json)
		{
			if (json == null || json.Content == null)
				return null;

            return base.PatchValue(patchedObject, expectedType, json);
        }
	}
}