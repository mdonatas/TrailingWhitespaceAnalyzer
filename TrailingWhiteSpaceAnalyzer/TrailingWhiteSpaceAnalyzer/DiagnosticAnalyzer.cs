using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace TrailingWhiteSpaceAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TrailingWhiteSpaceAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TrailingWhiteSpaceAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Formatting";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            IEnumerable<SyntaxTrivia> endOfLineNodes = from node in root.DescendantTrivia() where node.IsKind(SyntaxKind.EndOfLineTrivia) select node;

            foreach (var node in endOfLineNodes)
            {
                var location = node.GetLocation();
                var line = location.GetLineSpan().EndLinePosition.Line;

                if (node.Token.TrailingTrivia.Span.Length > 2)
                {
                    var diagnostic = Diagnostic.Create(Rule, Location.Create(node.SyntaxTree, node.Token.TrailingTrivia.Span), line);
                    context.ReportDiagnostic(diagnostic);
                }

                var leadingTrivia = node.Token.LeadingTrivia;
                if (leadingTrivia.Count > 1)
                {
                    var whitespaceSpan = default(TextSpan);
                    foreach (var trivia in leadingTrivia)
                    {
                        if ((SyntaxKind)trivia.RawKind == SyntaxKind.WhitespaceTrivia)
                        {
                            if (whitespaceSpan.End == 0)
                            {
                                whitespaceSpan = TextSpan.FromBounds(trivia.Span.Start, trivia.Span.End);
                            }
                            else
                            {
                                whitespaceSpan = TextSpan.FromBounds(whitespaceSpan.Start, trivia.Span.End);
                            }
                        }
                        else if ((SyntaxKind)trivia.RawKind == SyntaxKind.EndOfLineTrivia)
                        {
                            if (whitespaceSpan.End != 0)
                            {
                                var whitespaceLine = trivia.GetLocation().GetLineSpan().EndLinePosition.Line;
                                if (whitespaceLine == line)
                                {
                                    var diagnostic = Diagnostic.Create(Rule, Location.Create(node.SyntaxTree, whitespaceSpan), whitespaceLine);
                                    context.ReportDiagnostic(diagnostic);
                                }

                                whitespaceSpan = default(TextSpan);
                            }
                        }
                        else
                        {
                            whitespaceSpan = default(TextSpan);
                        }
                    }
                }
            }
        }
    }
}
