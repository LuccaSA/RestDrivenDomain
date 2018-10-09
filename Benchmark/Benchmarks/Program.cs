using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Querying;

namespace Benchmarks
{
    [CoreJob]
    [RankColumn]
    public class RddFieldsParsing
    {
        private FieldsParser _fieldsParser = new FieldsParser();
         
        [GlobalSetup]
        public void Setup()
        {
            _fieldsParser = new FieldsParser();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Fields))]
        public IExpressionTree<OrderEntity> StandardFields(string fields) => _fieldsParser.ParseFields<OrderEntity>(new Dictionary<string, string>
        {
            { Reserved.fields.ToString(),fields}
        },true);


        public IEnumerable<string> Fields()
        {
            yield return "id,name";
            yield return "OrderLines.Article.Reference";
            yield return "id,name,OrderLines[Id,Article[Reference,Weight],Quantity]";
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RddFieldsParsing>();
            Console.ReadKey();
        }
    }

    public class OrderEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public OrderLine OrderLines { get; set; }
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public Article Article { get; set; }
        public decimal Quantity { get; set; }
        public decimal VAT { get; set; }
    }

    public class Article
    {
        public String Reference { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public String Color { get; set; }
    }
}
