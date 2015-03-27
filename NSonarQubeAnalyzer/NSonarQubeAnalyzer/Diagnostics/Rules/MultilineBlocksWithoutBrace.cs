using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [SqaleConstantRemediation("5min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("bug")]
    public class MultilineBlocksWithoutBrace : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2681";
        internal const string Description = @"Multiline blocks should be enclosed in curly braces";
        internal const string MessageFormat = @"Only the first line of this n-line block will be executed {0}. The rest will execute {1}.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Critical; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c => CheckLoop(c, ((WhileStatementSyntax) c.Node).Statement),
                SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(
                c => CheckLoop(c, ((ForStatementSyntax) c.Node).Statement),
                SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(
                c => CheckLoop(c, ((ForEachStatementSyntax) c.Node).Statement),
                SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(
                c => CheckIf(c, (IfStatementSyntax) c.Node),
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

        private static void CheckStatement(SyntaxNodeAnalysisContext c, StatementSyntax statement, 
            string executed, string execute)
        {
            if (statement is BlockSyntax)
            {
                return;
            }

            var nextStatement = c.Node.GetLastToken().GetNextToken().Parent;
            if (nextStatement == null)
            {
                return;
            }

            var charPositionWithinLine1 = statement.GetLocation().GetLineSpan().StartLinePosition.Character;
            var charPositionWithinLine2 = nextStatement.GetLocation().GetLineSpan().StartLinePosition.Character;

            if (charPositionWithinLine1 == charPositionWithinLine2)
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule, nextStatement.GetLocation(), executed, execute));
            }
        }
    }
}
