using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class CatchRethrow : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2737";
        internal const string Description = @"""catch"" clauses should do more than rethrow";
        internal const string MessageFormat = @"Add logic to this catch clause or eliminate it and rethrow the exception automatically.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private readonly BlockSyntax _throwBlock =
            CSharpSyntaxTree.ParseText("{throw;}", new CSharpParseOptions(kind: SourceCodeKind.Interactive))
                .GetRoot()
                .DescendantNodes()
                .OfType<BlockSyntax>()
                .First();

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var tryStatement = (TryStatementSyntax)c.Node;

                    if (tryStatement.Catches.Count < 1)
                    {
                        return;
                    }

                    if (tryStatement.Catches.Count == 1 &&
                        SyntaxFactory.AreEquivalent(tryStatement.Catches[0].Block, _throwBlock))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                tryStatement.Catches[0].GetLocation()));
                        return;
                    }

                    var lastcatchClause = tryStatement.Catches.Last();

                    if (IsGenericExceptionClause(lastcatchClause) && 
                        SyntaxFactory.AreEquivalent(lastcatchClause.Block, _throwBlock))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                lastcatchClause.GetLocation()));
                    }
                },
                SyntaxKind.TryStatement);
        }

        private static bool IsGenericExceptionClause(CatchClauseSyntax catchClause)
        {
            return catchClause.Declaration == null ||
                   (catchClause.Declaration.Type.GetText().ToString() == "Exception");
        }
    }
}
