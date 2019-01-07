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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ShiftDynamicNotIntegerBase<TExpressionSyntax> : SonarDiagnosticAnalyzer
        where TExpressionSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S3449";
        protected const string MessageFormat = "Remove this erroneous shift, it will fail because '{0}' can't be implicitly converted to 'int'.";

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract bool CanBeConvertedTo(TExpressionSyntax expression, ITypeSymbol type, SemanticModel semanticModel);

        protected abstract bool ShouldRaise(SemanticModel semanticModel, TExpressionSyntax left, TExpressionSyntax right);

        protected void CheckExpressionWithTwoParts<T>(SyntaxNodeAnalysisContext context, Func<T, TExpressionSyntax> getLeft,
            Func<T, TExpressionSyntax> getRight)
            where T : SyntaxNode
        {
            var expression = (T)context.Node;
            var left = getLeft(expression);
            var right = getRight(expression);

            if (!IsErrorType(right, context.SemanticModel, out var typeOfRight) &&
                ShouldRaise(context.SemanticModel, left, right))
            {
                var typeInMessage = GetTypeNameForMessage(right, typeOfRight, context.SemanticModel);

                context.ReportDiagnosticWhenActive(
                    Diagnostic.Create(Rule, right.GetLocation(), typeInMessage));
            }
        }

        private static string GetTypeNameForMessage(SyntaxNode expression, ITypeSymbol typeOfRight, SemanticModel semanticModel)
        {
            var constValue = semanticModel.GetConstantValue(expression);
            return constValue.HasValue && constValue.Value == null
                ? "null"
                : typeOfRight.ToMinimalDisplayString(semanticModel, expression.SpanStart);
        }

        private bool IsErrorType(TExpressionSyntax expression, SemanticModel semanticModel, out ITypeSymbol type)
        {
            type = semanticModel.GetTypeInfo(expression).Type;
            return type.Is(TypeKind.Error);
        }

        protected bool IsConvertibleToInt(TExpressionSyntax expression, SemanticModel semanticModel)
        {
            var intType = semanticModel.Compilation.GetTypeByMetadataName("System.Int32");
            if (intType == null)
            {
                return false;
            }

            return CanBeConvertedTo(expression, intType, semanticModel);
        }
    }
}
