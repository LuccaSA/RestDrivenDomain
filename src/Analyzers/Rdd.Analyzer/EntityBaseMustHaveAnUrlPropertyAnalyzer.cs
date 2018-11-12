using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Rdd.Analyzer.Helpers;
using System.Linq;

namespace Rdd.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityBaseMustHaveAnUrlPropertyAnalyzer : RddAnalyzer
    {
        public const string DiagnosticId = "RDD001";
        public static readonly string Title = "EntityBase classes cannot explicitely implement property 'Url'.";
        public static readonly string Message = "'Url' property must be implicitely implemented.";
        private const string Category = "Design";

        protected override DiagnosticDescriptor DiagnosticDescriptor
            => new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, RddContext rddContext)
        {
            compilationStartContext.RegisterSymbolAction(context =>
            {
                var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
                var iEntityBaseSymbol = namedTypeSymbol.AllInterfaces.FirstOrDefault(i => i.Name == "IEntityBase" && !i.IsGenericType);
                if (iEntityBaseSymbol == null)
                    return;

                var implementation = namedTypeSymbol.FindImplementationForInterfaceMember(rddContext.CoreContext.IEntityBaseUrlMember);
                if (implementation == null || ((IPropertySymbol)implementation).ExplicitInterfaceImplementations.Length == 0)
                    return;

                var diagnostic = Diagnostic.Create(SupportedDiagnostics[0], implementation.Locations[0]);
                context.ReportDiagnostic(diagnostic);
            }, SymbolKind.NamedType);
        }
    }
}