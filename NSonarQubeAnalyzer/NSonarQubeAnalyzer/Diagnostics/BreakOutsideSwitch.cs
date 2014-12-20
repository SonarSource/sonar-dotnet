namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BreakOutsideSwitch : DiagnosticsRule
    {
        internal const string DiagnosticId = "BreakOutsideSwitch";
        internal const string Description = "'break' should not be used outside of 'switch'";
        internal const string MessageFormat = "Refactor the code in order to remove this break statement.";
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
                return "BreakOutsideSwitch";
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
                    BreakStatementSyntax breakNode = (BreakStatementSyntax)c.Node;
                    if (!IsInSwitch(breakNode))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, breakNode.GetLocation()));
                    }
                },
                SyntaxKind.BreakStatement);
        }

        private static bool IsInSwitch(BreakStatementSyntax node)
        {
            SyntaxNode ancestor = node.FirstAncestorOrSelf<SyntaxNode>(e =>
                e.IsKind(SyntaxKind.SwitchStatement) ||
                e.IsKind(SyntaxKind.WhileStatement) ||
                e.IsKind(SyntaxKind.DoStatement) ||
                e.IsKind(SyntaxKind.ForStatement) ||
                e.IsKind(SyntaxKind.ForEachStatement));

            return ancestor != null && ancestor.IsKind(SyntaxKind.SwitchStatement);
        }
    }
}