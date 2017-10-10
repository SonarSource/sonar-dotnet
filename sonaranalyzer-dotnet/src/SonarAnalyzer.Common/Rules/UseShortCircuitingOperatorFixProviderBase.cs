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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class UseShortCircuitingOperatorFixProviderBase<TBinaryExpression> : SonarCodeFixProvider
        where TBinaryExpression : SyntaxNode
    {
        internal const string Title = "Use short-circuiting operators";

        public override ImmutableArray<string> FixableDiagnosticIds =>  ImmutableArray.Create(UseShortCircuitingOperatorBase.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var expression = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as TBinaryExpression;
            if (expression == null ||
                !IsCandidateExpression(expression))
            {
                return TaskHelper.CompletedTask;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => ReplaceExpression(expression, root, context.Document)),
                context.Diagnostics);

            return TaskHelper.CompletedTask;
        }

        internal abstract bool IsCandidateExpression(TBinaryExpression expression);

        private Task<Document> ReplaceExpression(TBinaryExpression expression,
            SyntaxNode root, Document document)
        {
            var replacement = GetShortCircuitingExpressionNode(expression)
                .WithTriviaFrom(expression);
            var newRoot = root.ReplaceNode(expression, replacement);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        protected abstract TBinaryExpression GetShortCircuitingExpressionNode(TBinaryExpression expression);
    }
}
