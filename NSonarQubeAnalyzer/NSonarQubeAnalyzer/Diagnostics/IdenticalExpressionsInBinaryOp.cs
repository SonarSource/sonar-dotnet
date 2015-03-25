using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class IdenticalExpressionsInBinaryOp : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1764";
        internal const string Description = @"Identical expressions should not be used on both sides of a binary operator";
        internal const string MessageFormat = @"Identical sub-expressions on both sides of operator ""{0}""";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        internal static SyntaxKind[] SyntaxElementsToCheck = 
        {
            SyntaxKind.SubtractExpression,
            SyntaxKind.DivideExpression, SyntaxKind.ModuloExpression,
            SyntaxKind.LogicalOrExpression, SyntaxKind.LogicalAndExpression,
            SyntaxKind.BitwiseOrExpression, SyntaxKind.BitwiseAndExpression, SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LeftShiftExpression, SyntaxKind.RightShiftExpression
        };

        private readonly LiteralExpressionSyntax literalOneSyntax =
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(1));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var expression = (BinaryExpressionSyntax) c.Node;

                    if (expression.IsKind(SyntaxKind.LeftShiftExpression) &&
                        SyntaxFactory.AreEquivalent(expression.Right, literalOneSyntax))
                    {
                        return;
                    }

                    if (EquivalenceChecker.AreEquivalent(expression.Left, expression.Right, c.SemanticModel))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), expression.OperatorToken));
                    }
                },
                SyntaxElementsToCheck
                );
        }
    }
}