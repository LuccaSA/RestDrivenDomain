using Rdd.Domain.Exceptions;
using Rdd.Domain.Json;
using Rdd.Domain.Patchers;
using Rdd.Domain.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class PatchersTests
    {
        enum TestEnum { Mon, Tue, Wed };

        class ToPatch
        {
            public CustomFields CustomFields { get; set; }
            public Dictionary<int, int> IntDico { get; set; }
            public Dictionary<int, int?> IntNullableDico { get; set; }
            public Dictionary<int, string> StringDico { get; set; }
            public Dictionary<string, string> KeyStringValueStringDico { get; set; }

            public TestEnum DaysId { get; set; }
            public EnumClient<TestEnum> Days
            {
                get { return new EnumClient<TestEnum>(DaysId); }
                set { DaysId = (TestEnum)value.Id; }
            }

            public ICollection<object> ACollection { get; set; }

            public int[] ArrayOfInt { get; set; }
        }

        IPatcherProvider PatcherProvider;

        public PatchersTests()
        {
            PatcherProvider = new PatcherProvider();
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptNonNullValuesInDictionary()
        {
            var input = @"{intDico: { 1: 1, 2: 3 }, customFields: { 1: {code:""hey !""}, 2: {code:""salut""} }}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Equal(1, newPatched.IntDico[1]);
            Assert.Equal(3, newPatched.IntDico[2]);
            Assert.Equal("hey !", newPatched.CustomFields[1].Code);
            Assert.Equal("salut", newPatched.CustomFields[2].Code);
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptNullValuesInDictionary()
        {
            var input = @"{customFields: { 1: null, 2: {code:""test""} }}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Null(newPatched.CustomFields[1]);
            Assert.Equal("test", newPatched.CustomFields[2].Code);
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptNullValuesInKeyStringValueStringDico()
        {
            var input = @"{keyStringValueStringDico: { ""yo"": null, ""man"": ""test"" }}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Null(newPatched.KeyStringValueStringDico["yo"]);
            Assert.Equal("test", newPatched.KeyStringValueStringDico["man"]);
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptNullValuesOnNullableValueTypeInDictionary()
        {
            var input = @"{intNullableDico: { 1: null, 2: 3 }}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Null(newPatched.IntNullableDico[1]);
            Assert.Equal(3, newPatched.IntNullableDico[2]);
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptNullValuesOnStringDictionary()
        {
            var input = @"{stringDico: { 1: null, 2: ""test"" }}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Null(newPatched.StringDico[1]);
            Assert.Equal("test", newPatched.StringDico[2]);
        }

        [Fact]
        public void JsonParserShouldNotAcceptNullValuesEvenWhenNonNullableInDictionary()
        {
            Assert.Throws<BadRequestException>(() =>
            {
                var json = new JsonParser().Parse(@"{intDico: { 1: null, 2: 3 }}");
                IPatcher patcher = new ObjectPatcher(PatcherProvider);
                patcher.Patch(new ToPatch(), json);
            });
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptEnumClient()
        {
            var input = @"{days: {code: ""Tue""}}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Equal(TestEnum.Tue, newPatched.Days.Value);
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptCollection()
        {
            var input = @"{aCollection: [{mon: ""object""}, {autre: ""test""}]}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Equal(2, newPatched.ACollection.Count);
            Assert.Equal("object", (newPatched.ACollection.ElementAt(0) as Dictionary<string, object>)["mon"]);
            Assert.Equal("test", (newPatched.ACollection.ElementAt(1) as Dictionary<string, object>)["autre"]);
        }

        [Fact]
        public void PatchEntityHelperShouldAcceptArrays()
        {
            var input = @"{arrayOfInt: [1, 2, 3, 4]}";
            var json = new JsonParser().Parse(input);
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.Patch(newPatched, json);

            Assert.Equal(4, newPatched.ArrayOfInt.Length);
            Assert.Equal(1, newPatched.ArrayOfInt[0]);
            Assert.Equal(2, newPatched.ArrayOfInt[1]);
            Assert.Equal(3, newPatched.ArrayOfInt[2]);
            Assert.Equal(4, newPatched.ArrayOfInt[3]);
        }

        [Fact]
        public void PatchFromAnonymous()
        {
            var newPatched = new ToPatch();
            IPatcher patcher = new ObjectPatcher(PatcherProvider);
            patcher.PatchFromAnonymous(newPatched, new { arrayOfInt = new[] { 1, 2, 3, 4 } });

            Assert.Equal(4, newPatched.ArrayOfInt.Length);
            Assert.Equal(1, newPatched.ArrayOfInt[0]);
            Assert.Equal(2, newPatched.ArrayOfInt[1]);
            Assert.Equal(3, newPatched.ArrayOfInt[2]);
            Assert.Equal(4, newPatched.ArrayOfInt[3]);
        }
    }
}