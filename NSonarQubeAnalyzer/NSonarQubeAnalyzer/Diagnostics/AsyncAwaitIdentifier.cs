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
    public class AsyncAwaitIdentifier : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "AsyncAwaitIdentifier";
        internal const string Description = "'async' and 'await' should not be used as identifier";
        internal const string MessageFormat = "Rename this identifier.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        private static readonly IImmutableSet<string> ASYNC_OR_AWAIT = ImmutableHashSet.Create(new string[] {"async", "await"});

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    foreach (var asyncOrAwaitToken in GetAsyncOrAwaitTokens(c.Node))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, asyncOrAwaitToken.GetLocation()));
                    }
                },
                SyntaxKind.CompilationUnit);
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
