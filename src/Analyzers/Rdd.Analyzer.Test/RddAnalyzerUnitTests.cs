using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Rdd.Analyzer.Test
{
    public class UnitTest : CodeFixVerifier
    {
        [Fact]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using Rdd.Domain;

    namespace ConsoleApplication1
    {
        class TypeName : IEntityBase<int>
        {
            public int Id { get; set; }
            public string Name { get; }
            string IEntityBase.Url => null;

            public object GetId() => Id;
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = EntityBaseMustHaveAnUrlPropertyAnalyzer.DiagnosticId,
                Message = EntityBaseMustHaveAnUrlPropertyAnalyzer.Message,
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 32) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using Rdd.Domain;

    namespace ConsoleApplication1
    {
        class TypeName : IEntityBase<int>
        {
            public int Id { get; set; }
            public string Name { get; }
        public string Url { get; }

        public object GetId() => Id;
        }
    }";//the offset is corrected in reality
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new EntityBaseMustHaveAnUrlPropertyFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EntityBaseMustHaveAnUrlPropertyAnalyzer();
        }
    }
}