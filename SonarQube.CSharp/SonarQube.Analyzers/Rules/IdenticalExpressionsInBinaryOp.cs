using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [SqaleConstantRemediation("2min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class IdenticalExpressionsInBinaryOp : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1764";
        internal const string Description = @"Identical expressions should not be used on both sides of a binary operator";
        internal const string MessageFormat = @"Identical sub-expressions on both sides of operator ""{0}""";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Critical; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault);

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

                    using (var eqChecker = new EquivalenceChecker(c.SemanticModel))
                    {
                        if (eqChecker.AreEquivalent(expression.Left, expression.Right))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), expression.OperatorToken));
                        }
                    }

                },
                SyntaxElementsToCheck
                );
        }
    }
}