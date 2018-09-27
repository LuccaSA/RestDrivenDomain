﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Querying;
using RDD.Web.Serialization;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.Serializers;
using RDD.Web.Serialization.UrlProviders;
using RDD.Web.Tests.ServerMock;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class FieldsTests
    {
        private SerializerProvider GetSerializerProvider()
        {
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");

            return new SerializerProvider(
                new ReflectionProvider(new Mock<IMemoryCache>().Object),
                new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor),
                new List<IInheritanceConfiguration>());
        }

        [Fact]
        public void SpecialSelectionFields()
        {
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1> { new Obj1 { Id = 1 }, new Obj1 { Id = 2 } }, 1);

            var fields = new ExpressionParser().ParseTree<Obj1>("collection.count");

            var serializer = GetSerializerProvider();

            var json = serializer.ToJson(selection, fields);

            Assert.True(json.HasJsonValue("Count"));
            Assert.Equal("1", json.GetJsonValue("Count"));
        }
        [Fact]
        public void EmptyFieldsSelection()
        {
            var obj1 = new Obj1
            {
                Id = 1,
                Name = "1"
            };
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1> { obj1 }, 1);

            var fields = new FieldsParser().ParseFields<Obj1>("");

            var serializer = GetSerializerProvider();

            var json = serializer.ToJson(selection, fields);

            Assert.True(json.HasJsonValue("items.0.Id"));
            Assert.True(json.HasJsonValue("items.0.Name"));
            Assert.True(json.HasJsonValue("items.0.Url"));
        }
        [Fact]
        public void EmptyFieldsSingleObject()
        {
            var obj1 = new Obj1
            {
                Id = 1,
                Name = "1"
            };

            var fields = new FieldsParser().ParseFields<Obj1>("");

            var serializer = GetSerializerProvider();
            var json = serializer.ToJson(obj1, fields);

            Assert.True(json.HasJsonValue("Id"));
            Assert.True(json.HasJsonValue("Name"));
            Assert.True(json.HasJsonValue("Url"));
        }
        [Fact]
        public void TwoLevelSelection()
        {
            var obj1 = new Obj1
            {
                Id = 1,
                Name = "1",
                Obj2 = new Obj2
                {
                    Id = 2,
                    Name = "2",
                    Something = "something",
                    Else = "else",
                    Obj3 = new Obj3
                    {
                        Id = 3,
                        Name = "3",
                        Something = "A",
                        Else = "B"
                    }
                }
            };
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1> { obj1 }, 1);

            var fields = new FieldsParser().ParseFields<Obj1>("Obj2[Id,Name,Obj3[Something,Else],Else]");

            var serializer = GetSerializerProvider();
            var json = serializer.ToJson(selection, fields);

            Assert.True(json.HasJsonValue("items.0.Obj2.Obj3.Something"));
            Assert.True(json.HasJsonValue("items.0.Obj2.Obj3.Else"));
            Assert.True(json.HasJsonValue("items.0.Obj2.Else"));
            Assert.True(json.HasJsonValue("items.0.Obj2.Name"));
            Assert.False(json.HasJsonValue("items.0.Obj2.Obj3.Name"));
        }

        [Fact]
        public void SerializeSubCollections()
        {
            var serializer = GetSerializerProvider();

            var obj1 = new Obj1
            {
                Obj2s = new List<Obj2>
                {
                    new Obj2
                    {
                        Id = 1,
                        Name = "1"
                    },
                    new Obj2
                    {
                        Id = 2,
                        Name = "2"
                    }
                }
            };

            var selection = new Selection<Obj1>(new List<Obj1> { obj1 }, 1);
            var fields = new FieldsParser().ParseFields<Obj1>("obj2s[id,name]");

            var json = serializer.ToJson(selection, fields);

            Assert.True(json.HasJsonValue("items.0.Obj2s.0.Id"));
            Assert.True(json.HasJsonValue("items.0.Obj2s.0.Name"));
        }
    }

    public class Obj1 : IEntityBase<int>
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public string Url { get; }
        public Obj2 Obj2 { get; set; }

        public List<Obj2> Obj2s { get; set; }

        public object GetId() => Id;

        public void SetId(object id)
        {
            Id = (int)id;
        }
    }

    public class Obj2 : Obj1
    {
        public String Something { get; set; }
        public String Else { get; set; }
        public Obj3 Obj3 { get; set; }
    }

    public class Obj3 : Obj2
    {

    }
}
