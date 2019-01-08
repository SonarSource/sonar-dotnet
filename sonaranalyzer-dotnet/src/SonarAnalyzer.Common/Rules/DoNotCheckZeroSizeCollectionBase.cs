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
    public abstract class DoNotCheckZeroSizeCollectionBase<TLanguageKindEnum,
            TBinaryExpressionSyntax,
            TExpressionSyntax> : SonarDiagnosticAnalyzer
        where TLanguageKindEnum : struct
        where TBinaryExpressionSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3981";

        protected const string MessageFormat =
            "The {0} of '{1}' is always '>=0', so fix this test to get the real expected behavior.";

        protected abstract TLanguageKindEnum GreaterThanOrEqualExpression { get; }
        protected abstract TLanguageKindEnum LessThanOrEqualExpression { get; }
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract TExpressionSyntax GetLeftNode(TBinaryExpressionSyntax binaryExpression);

        protected abstract TExpressionSyntax GetRightNode(TBinaryExpressionSyntax binaryExpression);

        protected abstract ISymbol GetSymbol(SyntaxNodeAnalysisContext context, TExpressionSyntax expression);

        protected abstract string IEnumerableTString { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer,
                c =>
                {
                    var binaryExpression = (TBinaryExpressionSyntax)c.Node;
                    CheckCondition(c, GetLeftNode(binaryExpression), GetRightNode(binaryExpression));
                }, GreaterThanOrEqualExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer,
                c =>
                {
                    var binaryExpression = (TBinaryExpressionSyntax)c.Node;
                    CheckCondition(c, GetRightNode(binaryExpression), GetLeftNode(binaryExpression));
                }, LessThanOrEqualExpression);
        }

        private void CheckCondition(SyntaxNodeAnalysisContext context, TExpressionSyntax expressionValueNode,
            TExpressionSyntax constantValueNode)
        {
            if (!IsConstantZero(context, constantValueNode))
            {
                return;
            }

            var symbol = GetSymbol(context, expressionValueNode);
            if (symbol == null)
            {
                return;
            }

            var symbolType = GetDeclaringTypeName(symbol);
            if (symbolType == null)
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], context.Node.GetLocation(),
                symbol.Name.ToLowerInvariant(), symbolType));
        }

        private string GetDeclaringTypeName(ISymbol symbol)
        {
            if (IsArrayLengthProperty(symbol))
            {
                return "Array";
            }

            if (IsEnumerableCountMethod(symbol))
            {
                return IEnumerableTString;
            }

            if (IsCollectionProperty(symbol))
            {
                return "ICollection";
            }

            return null;
        }

        protected abstract TExpressionSyntax RemoveParentheses(TExpressionSyntax expression);

        private bool IsConstantZero(SyntaxNodeAnalysisContext context, TExpressionSyntax expression)
        {
            var constantExpressionNode = RemoveParentheses(expression);
            var constant = context.SemanticModel.GetConstantValue(constantExpressionNode);
            if (!constant.HasValue)
            {
                return false;
            }

            return (constant.Value is int) && (int)constant.Value == 0;
        }

        private static bool IsEnumerableCountMethod(ISymbol symbol)
        {
            var methodSymbol = symbol as IMethodSymbol;
            return methodSymbol.IsEnumerableCount();
        }

        private static bool IsArrayLengthProperty(ISymbol symbol)
        {
            return symbol is IPropertySymbol propertySymbol &&
                   propertySymbol.ContainingType.Is(KnownType.System_Array) &&
                   (propertySymbol.Name == nameof(Array.Length) || propertySymbol.Name == "LongLength");
        }

        private static bool IsCollectionProperty(ISymbol symbol)
        {
            return symbol is IPropertySymbol propertySymbol &&
                   propertySymbol.ContainingType.Implements(KnownType.System_Collections_Generic_ICollection_T) &&
                   propertySymbol.Name == nameof(System.Collections.Generic.ICollection<object>.Count);
        }
    }
}
