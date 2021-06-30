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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotCheckZeroSizeCollection : DoNotCheckZeroSizeCollectionBase<SyntaxKind, BinaryExpressionSyntax, ExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind GreaterThanOrEqualExpression => SyntaxKind.GreaterThanOrEqualExpression;
        protected override SyntaxKind LessThanOrEqualExpression => SyntaxKind.LessThanOrEqualExpression;
        protected override string IEnumerableTString { get; } = "IEnumerable<T>";

        protected override ExpressionSyntax GetLeftNode(BinaryExpressionSyntax binaryExpression) =>
            binaryExpression.Left;

        protected override ExpressionSyntax GetRightNode(BinaryExpressionSyntax binaryExpression) =>
            binaryExpression.Right;

        protected override ExpressionSyntax RemoveParentheses(ExpressionSyntax expression) =>
            expression.RemoveParentheses();

        protected override ISymbol GetSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            while (expression is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                expression = conditionalAccess.WhenNotNull;
            }

            return context.SemanticModel.GetSymbolInfo(expression).Symbol;
        }
    }
}
