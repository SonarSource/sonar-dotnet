using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class MultilineBlocksWithoutBrace : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2681";
        internal const string Description = @"Multiline blocks should be enclosed in curly braces";
        internal const string MessageFormat = @"Only the first line of this n-line block will be executed {0}. The rest will execute {1}.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var statement = GetStatement((WhileStatementSyntax)c.Node);
                    CheckLoop(c, statement);
                },
                SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var statement = GetStatement((ForStatementSyntax)c.Node);
                    CheckLoop(c, statement);
                },
                SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var statement = GetStatement((ForEachStatementSyntax)c.Node);
                    CheckLoop(c, statement);
                },
                SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    CheckIf(c, (IfStatementSyntax)c.Node);
                },
                SyntaxKind.IfStatement);
        }

        private static void CheckLoop(SyntaxNodeAnalysisContext c, StatementSyntax statement)
        {
            CheckStatement(c, statement, "in a loop", "only once");
        }

        private static void CheckIf(SyntaxNodeAnalysisContext c, IfStatementSyntax ifStatement)
        {
            CheckStatement(c, ifStatement.Else == null ? ifStatement.Statement : ifStatement.Else.Statement,
                "conditionally", "unconditionally");
        }

        private static void CheckStatement(SyntaxNodeAnalysisContext c, StatementSyntax statement, params object[] args)
        {
            if (statement is BlockSyntax)
            {
                return;
            }

            var nextStatement = c.Node.FollowingSyntaxNodes().FirstOrDefault();
            if (nextStatement == null)
            {
                return;
            }

            if (statement.HasLeadingTrivia ^ nextStatement.HasLeadingTrivia)
            {
                return;
            }

            var charPositionWithinLine1 = statement.GetLocation().GetLineSpan().StartLinePosition.Character;
            var charPositionWithinLine2 = nextStatement.GetLocation().GetLineSpan().StartLinePosition.Character;

            if (charPositionWithinLine1 == charPositionWithinLine2)
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, nextStatement.GetLocation(), args));
            }
        }

        #region Get statement for loops

        private static StatementSyntax GetStatement(ForStatementSyntax multilineStatement)
        {
            return multilineStatement.Statement;
        }
        private static StatementSyntax GetStatement(WhileStatementSyntax multilineStatement)
        {
            return multilineStatement.Statement;
        }
        private static StatementSyntax GetStatement(ForEachStatementSyntax multilineStatement)
        {
            return multilineStatement.Statement;
        }

        #endregion
    }
}
