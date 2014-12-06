using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Threading;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnnecessaryBooleanLiteral : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1125";
        internal const string Description = "Literal boolean values should not be used in condition expressions";
        internal const string MessageFormat = "Remove the literal \"{0}\" boolean value.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    LiteralExpressionSyntax literalNode = (LiteralExpressionSyntax)c.Node;
                    if (IsUnnecessary(literalNode))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, literalNode.GetLocation(), literalNode.Token.ToString()));
                    }
                },
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression);
        }

        private static bool IsUnnecessary(LiteralExpressionSyntax node)
        {
            return node.Parent.IsKind(SyntaxKind.EqualsExpression) ||
                node.Parent.IsKind(SyntaxKind.NotEqualsExpression) ||
                node.Parent.IsKind(SyntaxKind.LogicalAndExpression) ||
                node.Parent.IsKind(SyntaxKind.LogicalOrExpression) ||
                node.Parent.IsKind(SyntaxKind.LogicalNotExpression);
        }
    }
}
