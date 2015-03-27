using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoSqaleRemediation]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class AsyncAwaitIdentifier : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "AsyncAwaitIdentifier";
        internal const string Description = "'async' and 'await' should not be used as identifier";
        internal const string MessageFormat = "Rename this identifier.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major;
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        private static readonly IImmutableSet<string> AsyncOrAwait = ImmutableHashSet.Create(new string[] {"async", "await"});

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
                   AsyncOrAwait.Contains(token.ToString())
                   select token;
        }
    }
}
