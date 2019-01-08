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

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class NegatedIsExpressionCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Replace 'Not...Is...' with 'IsNot'.";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(NegatedIsExpression.DiagnosticId);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var unary = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as UnaryExpressionSyntax;

            if (!(unary?.Operand is BinaryExpressionSyntax isExpression) ||
                !isExpression.IsKind(SyntaxKind.IsExpression))
            {
                return TaskHelper.CompletedTask;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => ChangeToIsNotAsync(context.Document, unary, isExpression, c)),
                context.Diagnostics);

            return TaskHelper.CompletedTask;
        }

        private static async Task<Document> ChangeToIsNotAsync(Document document, UnaryExpressionSyntax unary, BinaryExpressionSyntax isExpression,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
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
