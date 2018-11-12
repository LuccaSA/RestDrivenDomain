using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rdd.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityBaseMustHaveAnUrlPropertyFixProvider)), Shared]
    public class EntityBaseMustHaveAnUrlPropertyFixProvider : CodeFixProvider
    {
        public const string Title = "Replace by implicit implementation";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(EntityBaseMustHaveAnUrlPropertyAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var declaration = root.FindNode(context.Span).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var urlMember = declaration.Members.OfType<PropertyDeclarationSyntax>().First(m => m.Identifier.ValueText == "Url");

            context.RegisterCodeFix(CodeAction.Create(Title, c => Fix(context.Document, urlMember, c), Title), context.Diagnostics);
        }

        private static PropertyDeclarationSyntax UrlProperty
            = PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("Url"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))));

        private static async Task<Document> Fix(Document document, PropertyDeclarationSyntax urlMember, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(urlMember, UrlProperty);
            return editor.GetChangedDocument();
        }
    }
}