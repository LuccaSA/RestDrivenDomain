using RDD.Domain.Json;
using System;
using System.Reflection;

namespace RDD.Domain.Patchers
{
    public interface IPatcher
    {
        object InitialValue(PropertyInfo property, object patchedObject);
        object PatchValue(object patchedObject, Type expectedType, IJsonElement json);
    }

    internal interface IPatcher<T> : IPatcher where T : IJsonElement
    {
        object PatchValue(object patchedObject, Type expectedType, T json);
    }

    public static class IPatcherExtension
    {
        public static object PatchFromAnonymous(this IPatcher patcher, object patchedObject, object anonymousObject)
        {
            if (anonymousObject == null)
                return patchedObject;

            return Patch(patcher, patchedObject, new JsonParser().ParseFromAnonymous(anonymousObject));
        }

        public static object Patch(this IPatcher patcher, object patchedObject, IJsonElement json)
        {
            if (patchedObject == null)
                throw new ArgumentNullException("patchedObject");

            return patcher.PatchValue(patchedObject, patchedObject.GetType(), json);
        }
    }
}
