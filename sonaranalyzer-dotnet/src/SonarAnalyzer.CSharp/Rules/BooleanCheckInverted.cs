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
using System.Collections.Immutable;
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
    public sealed class BooleanCheckInverted : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1940";
        private const string MessageFormat = "Use the opposite operator ('{0}') instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var expression = (BinaryExpressionSyntax)c.Node;
                    var enclosingSymbol = c.SemanticModel.GetEnclosingSymbol(expression.SpanStart) as IMethodSymbol;

                    if (enclosingSymbol?.MethodKind == MethodKind.UserDefinedOperator ||
                        IsIgnoredNullableOperation(expression, c.SemanticModel))
                    {
                        return;
                    }

                    var parenthesizedParent = expression.Parent;
                    while (parenthesizedParent is ParenthesizedExpressionSyntax)
                    {
                        parenthesizedParent = parenthesizedParent.Parent;
                    }

                    if (parenthesizedParent is PrefixUnaryExpressionSyntax logicalNot && logicalNot.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, logicalNot.GetLocation(),
                            OppositeTokens[expression.OperatorToken.Kind()]));
                    }
                },
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }

        private static bool IsIgnoredNullableOperation(BinaryExpressionSyntax expression, SemanticModel semanticModel)
        {
            return expression.OperatorToken.IsAnyKind(ignoredNullableOperators) &&
                (IsNullable(expression.Left, semanticModel) || IsNullable(expression.Right, semanticModel));
        }

        private static bool IsNullable(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            return semanticModel.GetSymbolInfo(expression).Symbol.GetSymbolType() is INamedTypeSymbol symbolType && symbolType.ConstructedFrom.Is(KnownType.System_Nullable_T);
        }

        private static readonly ISet<SyntaxKind> ignoredNullableOperators = new HashSet<SyntaxKind>
        {
            SyntaxKind.GreaterThanToken,
            SyntaxKind.GreaterThanEqualsToken,
            SyntaxKind.LessThanToken,
            SyntaxKind.LessThanEqualsToken
        };

        private static readonly Dictionary<SyntaxKind, string> OppositeTokens =
            new Dictionary<SyntaxKind, string>
            {
                {SyntaxKind.GreaterThanToken, "<="},
                {SyntaxKind.GreaterThanEqualsToken, "<"},
                {SyntaxKind.LessThanToken, ">="},
                {SyntaxKind.LessThanEqualsToken, ">"},
                {SyntaxKind.EqualsEqualsToken, "!="},
                {SyntaxKind.ExclamationEqualsToken, "=="}
            };
    }
}
