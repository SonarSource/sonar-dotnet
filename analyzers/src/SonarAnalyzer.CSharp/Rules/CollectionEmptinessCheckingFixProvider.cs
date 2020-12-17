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
                var type = CountType.FromExpression(binary);
                var countExpression = type.Left.HasValue
                    ? binary.Right as InvocationExpressionSyntax
                    : binary.Left as InvocationExpressionSyntax;

                if (countExpression is null || type.NoValues)
                {
                    return Task.CompletedTask;
                }
                else
                {
                    context.RegisterCodeFix(
                       CodeAction.Create(
                           Title,
                           c => Task.FromResult(Simplify(root, binary, countExpression, type, context))),
                           context.Diagnostics);

                    return Task.CompletedTask;
                }
            }
        }

        private Document Simplify(
            SyntaxNode root,
            BinaryExpressionSyntax binary,
            InvocationExpressionSyntax countExpression,
            CountType type,
            CodeFixContext context)
        {
            var countNode = countExpression.ChildNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
            var anyNode = countNode.WithName(SyntaxFactory.IdentifierName("Any"));
            ExpressionSyntax anyExpression = countExpression.ReplaceNode(countNode, anyNode);
            if (type.IsEmpty)
            {
                anyExpression = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, anyExpression);
            }
            return context.Document.WithSyntaxRoot(root.ReplaceNode(binary, anyExpression));
        }

        internal readonly struct CountType
        {
            public CountType(int? left, SyntaxKind logical, int? right )
            {
                Left = left;
                Right = right;
                LogicalOperator = logical;
            }

            public int? Left { get; }
            public int? Right { get; }
            public SyntaxKind LogicalOperator { get; }
            public bool NoValues => !Left.HasValue && !Right.HasValue;
            public bool IsEmpty => Empties.Contains(this);
            public override string ToString() => $"{Left} {LogicalOperator} {Right}";

            public static CountType FromExpression(BinaryExpressionSyntax binary)
            {
                int? left = default;
                int? right = default;
                if (binary.Left is LiteralExpressionSyntax l && ExpressionNumericConverter.TryGetConstantIntValue(l, out var l_out))
                {
                    left = l_out;
                };
                if (binary.Right is LiteralExpressionSyntax r && ExpressionNumericConverter.TryGetConstantIntValue(r, out var r_out))
                {
                    right = r_out;
                };
                return new CountType(left, binary.Kind(), right);
            }

            private static readonly CountType[] Empties = new[]
            {
                new CountType(default, SyntaxKind.EqualsEqualsToken, 0),
                new CountType(default, SyntaxKind.LessThanToken, 1),
                new CountType(default, SyntaxKind.LessThanEqualsToken, 0),
                new CountType(0, SyntaxKind.EqualsEqualsToken, default),
                new CountType(1, SyntaxKind.GreaterThanToken, default),
                new CountType(0, SyntaxKind.GreaterThanGreaterThanToken, default),
            };
        }
    }
}
