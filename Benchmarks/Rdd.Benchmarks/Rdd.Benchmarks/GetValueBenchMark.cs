using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Rdd.Benchmarks
{
    public class GetValueBenchmark
    {
        class User
        {
            public DateTime BirthDay { get; set; }
        }

        private User _user;
        private PropertyInfo _property;
        private ExpressionValueProvider _valueProvider;
        private ConcurrentDictionary<PropertyInfo, ExpressionValueProvider> _entries;
        private IMemoryCache _cache;

        [GlobalSetup]
        public void Setup()
        {
            _user = new User { BirthDay = new DateTime(2010, 1, 1) };
            _property = typeof(User).GetProperty(nameof(User.BirthDay));

            _entries = new ConcurrentDictionary<PropertyInfo, ExpressionValueProvider>();
            _valueProvider = new ExpressionValueProvider(_property);
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Benchmark(Baseline = true)]
        public object Reflection() => _property.GetValue(_user);

        [Benchmark]
        public object Expression() => _valueProvider.GetValue(_user);

        [Benchmark]
        public object CachedExpressionValueProvider() => _entries.GetOrAdd(_property, p => new ExpressionValueProvider(p)).GetValue(_user);

        [Benchmark]
        public object ExpressionValueProviderInMemoryCache()
            => _cache.GetOrCreate(_property, c => new ExpressionValueProvider(_property)).GetValue(_user);
    }
}