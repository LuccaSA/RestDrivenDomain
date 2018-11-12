using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Rdd.Analyzer.Helpers
{
    public class RddContext
    {
        public bool ShouldAnalyze { get; }
        public RddDomainContext CoreContext { get; }

        public RddContext(Compilation compilation)
        {
            ShouldAnalyze = compilation.ReferencedAssemblyNames.Any(a => a.Name.StartsWith("Rdd.", StringComparison.OrdinalIgnoreCase));
            CoreContext = new RddDomainContext(compilation);
        }
    }
}