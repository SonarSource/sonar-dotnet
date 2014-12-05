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
    public class IfConditionalAlwaysTrueOrFalse : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1145";
        internal const string Description = "\"if\" statement conditions should not unconditionally evaluate to \"true\" or to \"false\"";
        internal const string MessageFormat = "Replace this \"switch\" statement with \"if\" statements to increase readability.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
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
