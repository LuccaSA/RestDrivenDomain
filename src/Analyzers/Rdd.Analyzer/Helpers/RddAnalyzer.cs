using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Rdd.Analyzer.Helpers
{
    public abstract class RddAnalyzer : DiagnosticAnalyzer
    {
        protected abstract DiagnosticDescriptor DiagnosticDescriptor { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var rddContext = new RddContext(compilationStartContext.Compilation);
                if (rddContext.ShouldAnalyze)
                    AnalyzeCompilation(compilationStartContext, rddContext);
            });
        }

        internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, RddContext rddContext);
    }
}