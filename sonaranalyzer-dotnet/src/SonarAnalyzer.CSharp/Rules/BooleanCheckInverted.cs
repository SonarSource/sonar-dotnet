/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BooleanCheckInverted : BooleanCheckInvertedBase<BinaryExpressionSyntax>
    {
        private static readonly ISet<SyntaxKind> ignoredNullableOperators =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.GreaterThanToken,
                SyntaxKind.GreaterThanEqualsToken,
                SyntaxKind.LessThanToken,
                SyntaxKind.LessThanEqualsToken,
            };

        private static readonly Dictionary<SyntaxKind, string> oppositeTokens =
            new Dictionary<SyntaxKind, string>
            {
                { SyntaxKind.GreaterThanToken, "<=" },
                { SyntaxKind.GreaterThanEqualsToken, "<" },
                { SyntaxKind.LessThanToken, ">=" },
                { SyntaxKind.LessThanEqualsToken, ">" },
                { SyntaxKind.EqualsEqualsToken, "!=" },
                { SyntaxKind.ExclamationEqualsToken, "==" },
            };

        protected override DiagnosticDescriptor Rule { get; } =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                GetAnalysisAction(),
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);

        protected override bool IsIgnoredNullableOperation(BinaryExpressionSyntax expression, SemanticModel semanticModel) =>
            expression.OperatorToken.IsAnyKind(ignoredNullableOperators) &&
            (IsNullable(expression.Left, semanticModel) || IsNullable(expression.Right, semanticModel));

        protected override bool IsLogicalNot(BinaryExpressionSyntax expression, out SyntaxNode logicalNot)
        {
            var parenthesizedParent = expression.GetSelfOrTopParenthesizedExpression().Parent;
            var prefixUnaryExpression = parenthesizedParent as PrefixUnaryExpressionSyntax;

            logicalNot = prefixUnaryExpression;

            return prefixUnaryExpression != null
                && prefixUnaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationToken);
        }

        protected override string GetSuggestedReplacement(BinaryExpressionSyntax expression) =>
            oppositeTokens[expression.OperatorToken.Kind()];
    }
}
