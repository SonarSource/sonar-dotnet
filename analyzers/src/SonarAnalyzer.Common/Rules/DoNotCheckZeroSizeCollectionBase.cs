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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotCheckZeroSizeCollectionBase<TSyntaxKind, TBinaryExpressionSyntax, TExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TBinaryExpressionSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3981";
        private const string MessageFormat = "The {0} of '{1}' is always '>=0', so fix this test to get the real expected behavior.";

        private readonly DiagnosticDescriptor rule;

        protected abstract TSyntaxKind GreaterThanOrEqualExpression { get; }
        protected abstract TSyntaxKind LessThanOrEqualExpression { get; }
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract string IEnumerableTString { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected DoNotCheckZeroSizeCollectionBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected abstract TExpressionSyntax GetLeftNode(TBinaryExpressionSyntax binaryExpression);

        protected abstract TExpressionSyntax GetRightNode(TBinaryExpressionSyntax binaryExpression);

        protected abstract ISymbol GetSymbol(SyntaxNodeAnalysisContext context, TExpressionSyntax expression);

        protected abstract TExpressionSyntax RemoveParentheses(TExpressionSyntax expression);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var binaryExpression = (TBinaryExpressionSyntax)c.Node;
                    CheckCondition(c, GetLeftNode(binaryExpression), GetRightNode(binaryExpression));
                },
                GreaterThanOrEqualExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var binaryExpression = (TBinaryExpressionSyntax)c.Node;
                    CheckCondition(c, GetRightNode(binaryExpression), GetLeftNode(binaryExpression));
                },
                LessThanOrEqualExpression);
        }

        private void CheckCondition(SyntaxNodeAnalysisContext context, TExpressionSyntax expressionValueNode, TExpressionSyntax constantValueNode)
        {
            if (IsConstantZero(context, constantValueNode)
                && GetSymbol(context, expressionValueNode) is { } symbol
                && GetDeclaringTypeName(symbol) is { } symbolType)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], context.Node.GetLocation(), symbol.Name.ToLowerInvariant(), symbolType));
            }
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

            if (IsReadonlyCollectionProperty(symbol))
            {
                return "IReadonlyCollection";
            }

            return null;
        }

        private bool IsConstantZero(SyntaxNodeAnalysisContext context, TExpressionSyntax expression)
        {
            var constantExpressionNode = RemoveParentheses(expression);
            var constant = context.SemanticModel.GetConstantValue(constantExpressionNode);
            return constant.HasValue && (constant.Value is int intValue) && intValue == 0;
        }

        private static bool IsEnumerableCountMethod(ISymbol symbol) =>
            symbol is IMethodSymbol methodSymbol
            && methodSymbol.IsEnumerableCount();

        private static bool IsArrayLengthProperty(ISymbol symbol) =>
            symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.Is(KnownType.System_Array)
            && (propertySymbol.Name == nameof(Array.Length) || propertySymbol.Name == "LongLength");

        private static bool IsCollectionProperty(ISymbol symbol) =>
            symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.DerivesOrImplements(KnownType.System_Collections_Generic_ICollection_T)
            && propertySymbol.Name == nameof(System.Collections.Generic.ICollection<object>.Count);

        private static bool IsReadonlyCollectionProperty(ISymbol symbol) =>
            symbol is IPropertySymbol propertySymbol
            && propertySymbol.ContainingType.DerivesOrImplements(KnownType.System_Collections_Generic_IReadOnlyCollection_T)
            && propertySymbol.Name == nameof(System.Collections.Generic.IReadOnlyCollection<object>.Count);
    }
}
