/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public sealed class DoNotCheckZeroSizeCollection : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3981";
        private const string MessageFormat = "The {0} of '{1}' is always '>=0', so fix this test to get the real expected behavior.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var binaryExpression = (BinaryExpressionSyntax)c.Node;
                    CheckCondition(c, binaryExpression.Left, binaryExpression.Right);
                }, SyntaxKind.GreaterThanOrEqualExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var binaryExpression = (BinaryExpressionSyntax)c.Node;
                    CheckCondition(c, binaryExpression.Right, binaryExpression.Left);
                }, SyntaxKind.LessThanOrEqualExpression);
        }

        private static void CheckCondition(SyntaxNodeAnalysisContext context, ExpressionSyntax expressionValueNode,
            ExpressionSyntax constantValueNode)
        {
            if (!IsConstantZero(context, constantValueNode))
            {
                return;
            }

            ISymbol symbol = GetSymbol(context, expressionValueNode);
            if (symbol == null)
            {
                return;
            }

            string symbolType = GetDeclaringTypeName(symbol);
            if (symbolType == null)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(rule, context.Node.GetLocation(),
                symbol.Name.ToLowerInvariant(), symbolType));
        }

        private static ISymbol GetSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            while (true)
            {
                var conditionalAccess = expression as ConditionalAccessExpressionSyntax;
                if (conditionalAccess == null)
                {
                    break;
                }
                expression = conditionalAccess.WhenNotNull;
            }

            return context.SemanticModel.GetSymbolInfo(expression).Symbol;
        }

        private static string GetDeclaringTypeName(ISymbol symbol)
        {
            if (IsArrayLengthProperty(symbol))
            {
                return "Array";
            }

            if (IsEnumerableCountMethod(symbol))
            {
                return "IEnumerable<T>";
            }

            if (IsCollectionProperty(symbol))
            {
                return "ICollection";
            }

            return null;
        }

        private static bool IsConstantZero(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            var constantExpressionNode = expression.RemoveParentheses();
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
            var propertySymbol = symbol as IPropertySymbol;
            return propertySymbol != null &&
                   propertySymbol.ContainingType.Is(KnownType.System_Array) &&
                   (propertySymbol.Name == nameof(Array.Length) || propertySymbol.Name == "LongLength");
        }

        private static bool IsCollectionProperty(ISymbol symbol)
        {
            var propertySymbol = symbol as IPropertySymbol;
            return propertySymbol != null &&
                   propertySymbol.ContainingType.Implements(KnownType.System_Collections_Generic_ICollection_T) &&
                   propertySymbol.Name == nameof(System.Collections.Generic.ICollection<object>.Count);
        }
    }
}
