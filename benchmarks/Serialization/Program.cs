using BenchmarkDotNet.Running;
using System;

namespace Rdd.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Instanciators>();
            Console.ReadLine();
        }
    }
}
