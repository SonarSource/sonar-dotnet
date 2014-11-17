using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncAwaitIdentifier : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S1301";
        internal const string Description = "'async' and 'await' should not be used as identifier";
        internal const string MessageFormat = "Rename this identifier.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        private static readonly IImmutableSet<string> ASYNC_OR_AWAIT = ImmutableHashSet.Create(new string[] {"async", "await"});

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.CompilationUnit); } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            foreach (var asyncOrAwaitToken in GetAsyncOrAwaitTokens(node))
            {
                addDiagnostic(Diagnostic.Create(Rule, asyncOrAwaitToken.GetLocation()));
            }
        }

        private static IEnumerable<SyntaxToken> GetAsyncOrAwaitTokens(SyntaxNode node)
        {
            return from token in node.DescendantTokens()
                   where token.IsKind(SyntaxKind.IdentifierToken) &&
                   ASYNC_OR_AWAIT.Contains(token.ToString())
                   select token;
        }
    }
}
