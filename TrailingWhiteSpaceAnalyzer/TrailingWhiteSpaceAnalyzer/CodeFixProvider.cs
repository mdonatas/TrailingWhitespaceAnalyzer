using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace TrailingWhiteSpaceAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TrailingWhiteSpaceAnalyzerCodeFixProvider)), Shared]
    public class TrailingWhiteSpaceAnalyzerCodeFixProvider : CodeFixProvider
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CodeFixTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly Task CompletedTask = Task.FromResult(false);

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TrailingWhiteSpaceAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title.ToString(),
                    createChangedDocument: c => RemoveTrailingWhitespaceTriviaAsync(context.Document, diagnostic, c),
                    equivalenceKey: Title.ToString()),
                diagnostic);

            return CompletedTask;
        }

        private async Task<Document> RemoveTrailingWhitespaceTriviaAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxTrivia node = root.FindTrivia(diagnostic.Location.SourceSpan.Start, true);

            Document newDoc = document.WithSyntaxRoot(root.ReplaceTrivia(node, new SyntaxTrivia()));
            return newDoc;
        }
    }
}
