using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class ShortCircuitNullPointerDereference : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1697";
        internal const string Description = @"Short-circuit logic should be used to prevent null pointer dereferences in conditionals";
        internal const string MessageFormat = @"Either reverse the equality operator in the ""{0}"" null test, or reverse the logical operator that follows it.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private readonly ExpressionSyntax _nullExpression =
            CSharpSyntaxTree.ParseText("null", new CSharpParseOptions(kind: SourceCodeKind.Interactive))
                .GetRoot()
                .DescendantNodes()
                .OfType<ExpressionSyntax>()
                .First();

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var binaryExpression = (BinaryExpressionSyntax)c.Node;

                    var operatorText = "!=";

                    if (binaryExpression.OperatorToken.ValueText == "&&")
                    {
                        operatorText = "==";
                    }

                    ReportDereference(binaryExpression, operatorText, c);
                },
                SyntaxKind.LogicalOrExpression, SyntaxKind.LogicalAndExpression);
        }

        private void ReportDereference(BinaryExpressionSyntax binaryExpression, string operatorText, SyntaxNodeAnalysisContext c)
        {
            var binaryParent = binaryExpression.Parent as BinaryExpressionSyntax;
            if (binaryParent != null && binaryExpression.OperatorToken.ValueText == binaryParent.OperatorToken.ValueText)
            {
                return;
            }

            var expressionsInChain = ExpressionsInChain(binaryExpression).ToList();

            for (var i = 0; i < expressionsInChain.Count; i++)
            {
                var currentExpression = expressionsInChain[i];

                var comparisonToNull = currentExpression as BinaryExpressionSyntax;

                if (comparisonToNull == null || comparisonToNull.OperatorToken.ValueText != operatorText)
                {
                    continue;
                }

                var leftNull = SyntaxFactory.AreEquivalent(comparisonToNull.Left, _nullExpression);
                var rightNull = SyntaxFactory.AreEquivalent(comparisonToNull.Right, _nullExpression);

                if (leftNull && rightNull)
                {
                    continue;
                }

                if (!leftNull && !rightNull)
                {
                    continue;
                }

                var expressionComparedToNull = leftNull?comparisonToNull.Right:comparisonToNull.Left;

                for (var j = i + 1; j < expressionsInChain.Count; j++)
                {
                    var reported = ReportMatchingDereference(c, expressionsInChain[j], expressionComparedToNull, comparisonToNull);

                    if (reported)
                    {
                        break;
                    }
                }
            }
        }

        private static bool ReportMatchingDereference(SyntaxNodeAnalysisContext c, ExpressionSyntax subsequentExpression,
            ExpressionSyntax expressionComparedToNull, BinaryExpressionSyntax binary)
        {

            foreach (var descendant in subsequentExpression
                .DescendantNodes()
                .Where(n => SyntaxFactory.AreEquivalent(n, expressionComparedToNull)))
            {
                if (descendant.Parent is MemberAccessExpressionSyntax ||
                    descendant.Parent is ElementAccessExpressionSyntax)
                {
                    //found
                    c.ReportDiagnostic(Diagnostic.Create(Rule, binary.GetLocation(),
                        expressionComparedToNull.ToString()));
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<ExpressionSyntax> ExpressionsInChain(BinaryExpressionSyntax binaryExpression)
        {
            var expressionList = new List<ExpressionSyntax>();

            var currentBinary = binaryExpression;
            while (currentBinary != null)
            {
                expressionList.Add(currentBinary.Right);

                var leftBinary = currentBinary.Left as BinaryExpressionSyntax;
                if (leftBinary == null || leftBinary.OperatorToken.ValueText != binaryExpression.OperatorToken.ValueText)
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