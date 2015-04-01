using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Simplification;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [SqaleConstantRemediation("2min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("bug")]
    public class ShortCircuitNullPointerDereference : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1697";
        internal const string Description = @"Short-circuit logic should be used to prevent null pointer dereferences in conditionals";
        internal const string MessageFormat = @"Either reverse the equality operator in the ""{0}"" null test, or reverse the logical operator that follows it.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Blocker; 
        internal const bool IsActivatedByDefault = true;

        private readonly ExpressionSyntax nullExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binaryExpression = (BinaryExpressionSyntax)c.Node;

                    var comparisonOperator = SyntaxKind.ExclamationEqualsToken;

                    if (binaryExpression.OperatorToken.IsKind(SyntaxKind.AmpersandAmpersandToken))
                    {
                        comparisonOperator = SyntaxKind.EqualsEqualsToken;
                    }

                    ReportDereference(binaryExpression, comparisonOperator, c);
                },
                SyntaxKind.LogicalOrExpression, SyntaxKind.LogicalAndExpression);
        }

        private void ReportDereference(BinaryExpressionSyntax binaryExpression, SyntaxKind comparisonOperator, 
            SyntaxNodeAnalysisContext c)
        {
            if (IsMidLevelExpression(binaryExpression))
            {
                return;
            }

            var expressionsInChain = GetExpressionsInChain(binaryExpression).ToList();

            for (var i = 0; i < expressionsInChain.Count; i++)
            {
                var currentExpression = expressionsInChain[i];

                var comparisonToNull = currentExpression as BinaryExpressionSyntax;

                if (comparisonToNull == null || !comparisonToNull.OperatorToken.IsKind(comparisonOperator))
                {
                    continue;
                }

                var leftNull = SyntaxFactory.AreEquivalent(comparisonToNull.Left, nullExpression);
                var rightNull = SyntaxFactory.AreEquivalent(comparisonToNull.Right, nullExpression);

                if (leftNull && rightNull)
                {
                    continue;
                }

                if (!leftNull && !rightNull)
                {
                    continue;
                }

                var expressionComparedToNull = leftNull ? comparisonToNull.Right : comparisonToNull.Left;
                CheckFollowingExpressions(c, i, expressionsInChain, expressionComparedToNull, comparisonToNull);
            }
        }

        private static bool IsMidLevelExpression(BinaryExpressionSyntax binaryExpression)
        {
            var binaryParent = binaryExpression.Parent as BinaryExpressionSyntax;
            return binaryParent != null && 
                   SyntaxFactory.AreEquivalent(binaryExpression.OperatorToken, binaryParent.OperatorToken);
        }

        private static void CheckFollowingExpressions(SyntaxNodeAnalysisContext c, int currentExpressionIndex,
            List<ExpressionSyntax> expressionsInChain,
            ExpressionSyntax expressionComparedToNull, BinaryExpressionSyntax comparisonToNull)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var expandedExpressionComparedToNull = Simplifier.Expand(expressionComparedToNull, c.SemanticModel,
                    workspace);

                using (var eqChecker = new EquivalenceChecker(c.SemanticModel))
                {
                    for (var j = currentExpressionIndex + 1; j < expressionsInChain.Count; j++)
                    {
                        if (expressionsInChain[j]
                            .DescendantNodes()
                            .Where(descendant => descendant.IsKind(expressionComparedToNull.Kind()) &&
                                                 eqChecker.AreEquivalent(
                                                     expandedExpressionComparedToNull, descendant, false))
                            .Where(descendant =>
                                descendant.Parent is MemberAccessExpressionSyntax ||
                                descendant.Parent is ElementAccessExpressionSyntax)
                            .ToList()
                            .Any())
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, comparisonToNull.GetLocation(),
                                expressionComparedToNull.ToString()));
                        }
                    }
                }
            }
        }

        private static IEnumerable<ExpressionSyntax> GetExpressionsInChain(BinaryExpressionSyntax binaryExpression)
        {
            var expressionList = new List<ExpressionSyntax>();

            var currentBinary = binaryExpression;
            while (currentBinary != null)
            {
                expressionList.Add(currentBinary.Right);

                var leftBinary = currentBinary.Left as BinaryExpressionSyntax;
                if (leftBinary == null || 
                    !SyntaxFactory.AreEquivalent(leftBinary.OperatorToken, binaryExpression.OperatorToken))
                {
                    expressionList.Add(currentBinary.Left);
                    break;
                }

                currentBinary = leftBinary;
            }

            expressionList.Reverse();
            return expressionList;
        }
    }
}