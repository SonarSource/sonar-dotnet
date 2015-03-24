using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class CatchRethrow : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2737";
        internal const string Description = @"""catch"" clauses should do more than rethrow";
        internal const string MessageFormat = @"Add logic to this catch clause or eliminate it and rethrow the exception automatically.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private readonly BlockSyntax throwBlock = SyntaxFactory.Block(SyntaxFactory.ThrowStatement());

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var tryStatement = (TryStatementSyntax)c.Node;
                    
                    var lastCatchClause = tryStatement.Catches.LastOrDefault();

                    if (lastCatchClause == null)
                    {
                        return;
                    }

                    if (SyntaxFactory.AreEquivalent(lastCatchClause.Block, throwBlock))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                lastCatchClause.GetLocation()));
                    }
                },
                SyntaxKind.TryStatement);
        }
    }
}
