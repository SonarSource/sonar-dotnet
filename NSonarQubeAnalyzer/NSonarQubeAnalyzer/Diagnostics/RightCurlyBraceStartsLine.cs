using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RightCurlyBraceStartsLine : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1109";
        internal const string Description = "A close curly brace should be located at the beginning of a line";
        internal const string MessageFormat = "Move this closing curly brace to the next line.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    foreach (var closeBraceToken in GetDescendantCloseBraceTokens(c.Node))
                    {
                        if (!StartsLine(closeBraceToken) && !IsOnSameLineAsOpenBrace(closeBraceToken) && !IsInitializer(closeBraceToken.Parent))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, closeBraceToken.GetLocation()));
                        }
                    }
                },
                SyntaxKind.CompilationUnit);
        }

        private static bool StartsLine(SyntaxToken token)
        {
            return token.GetPreviousToken().GetLocation().GetLineSpan().EndLinePosition.Line != token.GetLocation().GetLineSpan().StartLinePosition.Line;
        }

        private static bool IsOnSameLineAsOpenBrace(SyntaxToken closeBraceToken)
        {
            var openBraceToken = closeBraceToken.Parent.ChildTokens().Single(token => token.IsKind(SyntaxKind.OpenBraceToken));
            return openBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line == closeBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        }

        private static bool IsInitializer(SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.ArrayInitializerExpression) ||
                node.IsKind(SyntaxKind.CollectionInitializerExpression) ||
                node.IsKind(SyntaxKind.AnonymousObjectCreationExpression) ||
                node.IsKind(SyntaxKind.ObjectInitializerExpression);
        }

        private static IEnumerable<SyntaxToken> GetDescendantCloseBraceTokens(SyntaxNode node)
        {
            return node.DescendantTokens().Where(token => token.IsKind(SyntaxKind.CloseBraceToken));
        }
    }
}
