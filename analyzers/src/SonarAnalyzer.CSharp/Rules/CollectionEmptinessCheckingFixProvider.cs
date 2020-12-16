/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CollectionEmptinessCheckingFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Use Any() instead";
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(CollectionEmptinessChecking.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var binary = root.FindNode(diagnosticSpan)?.FirstAncestorOrSelf<BinaryExpressionSyntax>();

            if (binary is null)
            {
                return Task.CompletedTask;
            }
            else
            {
                var countExpression = binary.Left as InvocationExpressionSyntax;
                var literal = binary.Right as LiteralExpressionSyntax;
                var invocationFirst = countExpression != null;

                if (!invocationFirst)
                {
                    literal = binary.Left as LiteralExpressionSyntax;
                    countExpression = binary.Right as InvocationExpressionSyntax;
                }

                if (countExpression is null || literal is null)
                {
                    return Task.CompletedTask;
                }
                else
                {
                    var isEmpty = IsEmpty(literal, binary.OperatorToken, invocationFirst);
                    context.RegisterCodeFix(
                       CodeAction.Create(
                           Title,
                           c => Task.FromResult(Simplify(root, binary, countExpression, isEmpty, context))),
                           context.Diagnostics);

                    return Task.CompletedTask;
                }
            }
        }

        private Document Simplify(
            SyntaxNode root,
            BinaryExpressionSyntax binary,
            InvocationExpressionSyntax countExpression,
            bool isEmpty,
            CodeFixContext context)
        {
            var countNode = countExpression.ChildNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
            var anyNode = countNode.WithName(SyntaxFactory.IdentifierName("Any"));
            ExpressionSyntax anyExpression = countExpression.ReplaceNode(countNode, anyNode);
            if (isEmpty)
            {
                anyExpression = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, anyExpression);
            }
            return context.Document.WithSyntaxRoot(root.ReplaceNode(binary, anyExpression));
        }

        private static bool IsEmpty(LiteralExpressionSyntax literal, SyntaxToken logicalOperator, bool invocationFirst)
            => EqualsZero(literal, logicalOperator)
            || (invocationFirst ? LessThenOne(literal, logicalOperator) : OneGreaterThan(literal, logicalOperator));

        private static bool EqualsZero(LiteralExpressionSyntax literal, SyntaxToken logicalOperator)
            => logicalOperator.IsKind(SyntaxKind.EqualsEqualsToken) &&
                ExpressionNumericConverter.TryGetConstantIntValue(literal, out var value) &&
                value == 0;

        private static bool LessThenOne(LiteralExpressionSyntax literal, SyntaxToken logicalOperator)
            => logicalOperator.IsKind(SyntaxKind.LessThanToken) &&
                ExpressionNumericConverter.TryGetConstantIntValue(literal, out var value) &&
                value == 1;

        private static bool OneGreaterThan(LiteralExpressionSyntax literal, SyntaxToken logicalOperator)
            => logicalOperator.IsKind(SyntaxKind.GreaterThanToken) &&
                ExpressionNumericConverter.TryGetConstantIntValue(literal, out var value) &&
                value == 1;
    }
}
