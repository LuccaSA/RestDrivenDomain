using RDD.Domain.Json;
using System;

namespace RDD.Domain.Patchers
{
    public interface IPatcherProvider
    {
        IPatcher GetPatcher(Type expectedType, IJsonElement json);
    }
}
