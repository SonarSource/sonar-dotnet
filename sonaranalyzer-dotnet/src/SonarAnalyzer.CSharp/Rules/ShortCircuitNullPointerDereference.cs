/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [Obsolete("This rule is superseded by S2259.")]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ShortCircuitNullPointerDereference : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1697";
        private const string MessageFormat =
            "Either reverse the equality operator in the '{0}' null test, or reverse the logical operator that follows it.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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

        private static void ReportDereference(BinaryExpressionSyntax binaryExpression, SyntaxKind comparisonOperator,
            SyntaxNodeAnalysisContext context)
        {
            if (IsMidLevelExpression(binaryExpression))
            {
                return;
            }

            var expressionsInChain = GetExpressionsInChain(binaryExpression).ToList();

            for (var i = 0; i < expressionsInChain.Count; i++)
            {
                var currentExpression = expressionsInChain[i];


                if (!(currentExpression is BinaryExpressionSyntax comparisonToNull) || !comparisonToNull.OperatorToken.IsKind(comparisonOperator))
                {
                    continue;
                }

                var leftNull = SyntaxFactory.AreEquivalent(comparisonToNull.Left, CSharpSyntaxHelper.NullLiteralExpression);
                var rightNull = SyntaxFactory.AreEquivalent(comparisonToNull.Right, CSharpSyntaxHelper.NullLiteralExpression);

                if (leftNull && rightNull)
                {
                    continue;
                }

                if (!leftNull && !rightNull)
                {
                    continue;
                }

                var expressionComparedToNull = leftNull ? comparisonToNull.Right : comparisonToNull.Left;
                CheckFollowingExpressions(context, i, expressionsInChain, expressionComparedToNull, comparisonToNull);
            }
        }

        private static bool IsMidLevelExpression(BinaryExpressionSyntax binaryExpression)
        {
            return binaryExpression.Parent is BinaryExpressionSyntax binaryParent &&
                   SyntaxFactory.AreEquivalent(binaryExpression.OperatorToken, binaryParent.OperatorToken);
        }

        private static void CheckFollowingExpressions(SyntaxNodeAnalysisContext context, int currentExpressionIndex,
            IList<ExpressionSyntax> expressionsInChain,
            ExpressionSyntax expressionComparedToNull, BinaryExpressionSyntax comparisonToNull)
        {
            for (var j = currentExpressionIndex + 1; j < expressionsInChain.Count; j++)
            {
                var descendantNodes = expressionsInChain[j].DescendantNodes()
                    .Where(descendant =>
                        descendant.IsKind(expressionComparedToNull.Kind()) &&
                        CSharpEquivalenceChecker.AreEquivalent(expressionComparedToNull, descendant))
                        .Where(descendant =>
                    (descendant.Parent is MemberAccessExpressionSyntax &&
                        CSharpEquivalenceChecker.AreEquivalent(expressionComparedToNull,
                            ((MemberAccessExpressionSyntax) descendant.Parent).Expression)) ||
                    (descendant.Parent is ElementAccessExpressionSyntax &&
                        CSharpEquivalenceChecker.AreEquivalent(expressionComparedToNull,
                            ((ElementAccessExpressionSyntax) descendant.Parent).Expression)))
                    .ToList();

                if (descendantNodes.Any())
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, comparisonToNull.GetLocation(),
                        expressionComparedToNull.ToString()));
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

                if (!(currentBinary.Left is BinaryExpressionSyntax leftBinary) ||
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
