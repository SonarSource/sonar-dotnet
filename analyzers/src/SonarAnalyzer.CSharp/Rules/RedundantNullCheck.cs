/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class RedundantNullCheck : RedundantNullCheckBase<BinaryExpressionSyntax>
    {
        private const string MessageFormat = "Remove this unnecessary null check; 'is' returns false for nulls.";
        private const string MessageFormatForPatterns = "Remove this unnecessary null check; it is already done by the pattern match.";

        private static readonly DiagnosticDescriptor RuleForIs =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly DiagnosticDescriptor RuleForPatternSyntax =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormatForPatterns, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RuleForIs, RuleForPatternSyntax);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndExpression,
                SyntaxKind.LogicalAndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrExpression,
                SyntaxKind.LogicalOrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndPattern,
                SyntaxKindEx.AndPattern);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrPattern,
                SyntaxKindEx.OrPattern);
        }

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left.RemoveParentheses();

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right.RemoveParentheses();

        protected override SyntaxNode GetNullCheckVariable(SyntaxNode node) => GetNullCheckVariable(node, true);

        protected override SyntaxNode GetNonNullCheckVariable(SyntaxNode node) => GetNullCheckVariable(node, false);

        protected override SyntaxNode GetIsOperatorCheckVariable(SyntaxNode node)
        {
            var innerExpression = node.RemoveParentheses();
            if (innerExpression is BinaryExpressionSyntax binaryExpression && binaryExpression.IsKind(SyntaxKind.IsExpression))
            {
                return GetLeftNode(binaryExpression);
            }
            else if (innerExpression.IsKind(SyntaxKindEx.IsPatternExpression))
            {
                var isPatternExpression = (IsPatternExpressionSyntaxWrapper)innerExpression.RemoveParentheses();
                if (IsAffirmativePatternMatch(isPatternExpression))
                {
                    return isPatternExpression.Expression.RemoveParentheses();
                }
            }
            return null;

            // Verifies the given pattern is like "foo is Bar" - where Bar can be various Patterns, except 'null'.
            static bool IsAffirmativePatternMatch(IsPatternExpressionSyntaxWrapper isPatternWrapper) =>
                !isPatternWrapper.IsNull() && !isPatternWrapper.IsNot();
        }

        protected override SyntaxNode GetInvertedIsOperatorCheckVariable(SyntaxNode node)
        {
            var innerExpression = node.RemoveParentheses();
            if (innerExpression is PrefixUnaryExpressionSyntax prefixUnary && prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return GetIsOperatorCheckVariable(prefixUnary.Operand);
            }
            else if (innerExpression.IsKind(SyntaxKindEx.IsPatternExpression))
            {
                var isPatternExpression = (IsPatternExpressionSyntaxWrapper)innerExpression.RemoveParentheses();
                if (IsNegativePatternMatch(isPatternExpression))
                {
                    return isPatternExpression.Expression.RemoveParentheses();
                }
            }
            return null;

            // Verifies the pattern is like "foo is not Bar" - where Bar can be various Patterns, except 'null'.
            static bool IsNegativePatternMatch(IsPatternExpressionSyntaxWrapper patternSyntaxWrapper) =>
                patternSyntaxWrapper.IsNot() && !patternSyntaxWrapper.IsNotNull();
        }

        protected override bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) => CSharpEquivalenceChecker.AreEquivalent(node1, node2);

        private static void CheckAndPattern(SyntaxNodeAnalysisContext context)
        {
            var binaryPatternNode = (BinaryPatternSyntaxWrapper)context.Node;
            var left = binaryPatternNode.Left.SyntaxNode.RemoveParentheses();
            var right = binaryPatternNode.Right.SyntaxNode.RemoveParentheses();

            if (IsNotNullPattern(left) && IsAffirmativePatternMatch(right))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(RuleForPatternSyntax, left.GetLocation()));
            }
            else if (IsNotNullPattern(right) && IsAffirmativePatternMatch(left))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(RuleForPatternSyntax, right.GetLocation()));
            }

            static bool IsNotNullPattern(SyntaxNode node) =>
                UnaryPatternSyntaxWrapper.IsInstance(node)
                && ((UnaryPatternSyntaxWrapper)node) is var unaryPatternSyntaxWrapper
                && unaryPatternSyntaxWrapper.IsNotNull();

            // Verifies the given pattern is an affirmative pattern - constant pattern (except 'null'), Declaration pattern, Recursive pattern.
            // The PatternSyntax appears e.g. in switch arms and is different from IsPatternSyntax.
            static bool IsAffirmativePatternMatch(SyntaxNode node) =>
                PatternSyntaxWrapper.IsInstance(node)
                && ((PatternSyntaxWrapper)node) is var isPatternWrapper
                && !isPatternWrapper.IsNot()
                && !isPatternWrapper.IsNull();
        }

        private static void CheckOrPattern(SyntaxNodeAnalysisContext context)
        {
            var binaryPatternNode = (BinaryPatternSyntaxWrapper)context.Node;
            var left = binaryPatternNode.Left.SyntaxNode.RemoveParentheses();
            var right = binaryPatternNode.Right.SyntaxNode.RemoveParentheses();
            if (PatternSyntaxWrapper.IsInstance(left) && PatternSyntaxWrapper.IsInstance(right))
            {
                var leftPattern = (PatternSyntaxWrapper)left;
                var rightPattern = (PatternSyntaxWrapper)right;
                if (leftPattern.IsNull() && rightPattern.IsNot())
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(RuleForPatternSyntax, left.GetLocation()));
                }
                else if (rightPattern.IsNull() && leftPattern.IsNot())
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(RuleForPatternSyntax, right.GetLocation()));
                }
            }
        }

        /// <summary>
        /// Retrieves the variable that gets null-checked only if the null-check respects the expectation of the caller (it is either an affirmative or a negative check).
        /// For example:
        /// - if the node is "foo is null" / "foo == null" and expectedAffirmative is "true", the method will return "foo".
        /// - if the node is "foo is null" / "foo == null" and expectedAffirmative is "false", the method will return null.
        /// - if the node is "foo is not null" / "foo != null" / "!(foo is null)" and expectedAffirmative is "true", the method will return null.
        /// - if the node is "foo is not null" / "foo != null" / "!(foo is null)" and expectedAffirmative is "false", the method will return "foo".
        /// </summary>
        private static SyntaxNode GetNullCheckVariable(SyntaxNode node, bool expectedAffirmative)
        {
            var innerExpression = node.RemoveParentheses();
            if (innerExpression is PrefixUnaryExpressionSyntax prefixUnary && prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                innerExpression = prefixUnary.Operand;
                expectedAffirmative = !expectedAffirmative;
            }
            if (((ExpressionSyntax)innerExpression).TryGetExpressionComparedToNull(out var compared, out var actualAffirmative)
                && actualAffirmative == expectedAffirmative)
            {
                return compared;
            }
            return null;
        }
    }
}
