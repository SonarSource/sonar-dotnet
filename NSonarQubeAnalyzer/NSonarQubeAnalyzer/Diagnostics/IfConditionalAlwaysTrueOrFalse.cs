namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IfConditionalAlwaysTrueOrFalse : DiagnosticsRule
    {
        internal const string DiagnosticId = "S1145";
        internal const string Description = "\"if\" statement conditions should not unconditionally evaluate to \"true\" or to \"false\"";
        internal const string MessageFormat = "Replace this \"switch\" statement with \"if\" statements to increase readability.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        /// <summary>
        /// Rule ID
        /// </summary>
        public override string RuleId
        {
            get
            {
                return "S1145";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            if (!Status)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    IfStatementSyntax ifNode = (IfStatementSyntax)c.Node;

                    if (HasBooleanLiteralExpressionAsCondition(ifNode))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, ifNode.GetLocation()));
                    }
                },
                SyntaxKind.IfStatement);
        }

        private static bool HasBooleanLiteralExpressionAsCondition(IfStatementSyntax node)
        {
            return node.Condition.IsKind(SyntaxKind.TrueLiteralExpression) ||
                node.Condition.IsKind(SyntaxKind.FalseLiteralExpression);
        }
    }
}