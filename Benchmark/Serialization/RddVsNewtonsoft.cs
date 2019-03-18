using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Helpers;
using Rdd.Web.Serialization.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rdd.Benchmarks
{
    [MemoryDiagnoser]
    public class RddVsNewtonsoft
    {
        object _value;
        IExpressionTree _fields;
        TextWriter _writer;
        IServiceProvider _servicesProvider;
        ISerializerProvider _serializer;
        JsonSerializer _jsonSerializer;

        class User
        {
            public int Id { get; set; }
            public DateTime BirthDay { get; set; }
            public List<Other> Properties { get; set; }
            public string Token { get; set; }
            public string Name { get; set; }
        }

        class Other
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [GlobalSetup]
        public void Setup()
        {
            _value = Enumerable.Range(0, 100).Select(i => new User { Id = i, BirthDay = new DateTime(2000, 1, 1), Properties = Enumerable.Range(0, 100).Select(j => new Other { Id = j, Name = i + "  " + j }).ToList() }).ToList();

            var services = new ServiceCollection();
            var builder = new RddBuilder(services);

            _servicesProvider = services.BuildServiceProvider();
            _serializer = _servicesProvider.GetService<ISerializerProvider>();
            _fields = new ExpressionParser().ParseTree(typeof(User), "id,birthday,properties[id,name],token,name");
            _jsonSerializer = JsonSerializer.CreateDefault();

            _writer = StreamWriter.Null;
        }

        [Benchmark]
        public void Rdd()
        {
            using (var jsonWriter = new JsonTextWriter(_writer) { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified })
            {
                jsonWriter.CloseOutput = false;
                jsonWriter.AutoCompleteOnClose = false;

                _serializer.WriteJson(jsonWriter, _value, _fields);
            }
        }

        [Benchmark(Baseline = true)]
        public void Newtonsoft()
        {
            using (var jsonWriter = new JsonTextWriter(_writer))
            {
                jsonWriter.CloseOutput = false;
                jsonWriter.AutoCompleteOnClose = false;

                _jsonSerializer.Serialize(jsonWriter, _value);
            }
        }
    }
}