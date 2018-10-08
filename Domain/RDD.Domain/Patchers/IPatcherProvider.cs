using Rdd.Domain.Json;
using System;

namespace Rdd.Domain.Patchers
{
    public interface IPatcherProvider
    {
        IPatcher GetPatcher(Type expectedType, IJsonElement json);
    }
}
