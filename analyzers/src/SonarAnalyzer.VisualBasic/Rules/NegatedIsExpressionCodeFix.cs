/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class NegatedIsExpressionCodeFix : SonarCodeFix
    {
        internal const string Title = "Replace 'Not...Is...' with 'IsNot'.";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NegatedIsExpression.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var unary = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as UnaryExpressionSyntax;

            if (!(unary?.Operand is BinaryExpressionSyntax isExpression) ||
                !isExpression.IsKind(SyntaxKind.IsExpression))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c => ChangeToIsNotAsync(context.Document, unary, isExpression, c),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static async Task<Document> ChangeToIsNotAsync(Document document, UnaryExpressionSyntax unary, BinaryExpressionSyntax isExpression, CancellationToken cancel)
        {
            var root = await document.GetSyntaxRootAsync(cancel).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(
                unary,
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.IsNotExpression,
                    isExpression.Left,
                    SyntaxFactory.Token(SyntaxKind.IsNotKeyword).WithTriviaFrom(isExpression.OperatorToken),
                    isExpression.Right));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
