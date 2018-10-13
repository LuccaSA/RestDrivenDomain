using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Rdd.Benchmarks
{
    public class GetPropertiesBenchmarks
    {
        private Type _type;
        private ConcurrentDictionary<Type, PropertyInfo[]> _entries;
        private IMemoryCache _cache;

        [GlobalSetup]
        public void Setup()
        {
            _type = typeof(PropertyInfo);
            _entries = new ConcurrentDictionary<Type, PropertyInfo[]>();
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Benchmark(Baseline = true)]
        public object Reflection() => _type.GetProperties();

        [Benchmark]
        public object ActualReflectionProvider() => _cache.GetOrCreate("reflectionProvider:" + _type.AssemblyQualifiedName, c => _type.GetProperties());

        [Benchmark]
        public object MemoryCacheBetterKey() => _cache.GetOrCreate(_type, c => _type.GetProperties());

        [Benchmark]
        public object Cache() => _entries.GetOrAdd(_type, t => t.GetProperties());
    }
}