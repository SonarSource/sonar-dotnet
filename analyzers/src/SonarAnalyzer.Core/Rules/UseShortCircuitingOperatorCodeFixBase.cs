/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
{
    public abstract class UseShortCircuitingOperatorCodeFixBase<TSyntaxKind, TBinaryExpression> : SonarCodeFix where TSyntaxKind : struct
        where TBinaryExpression : SyntaxNode
    {
        internal const string Title = "Use short-circuiting operators";

        public override ImmutableArray<string> FixableDiagnosticIds =>  ImmutableArray.Create(UseShortCircuitingOperatorBase<TSyntaxKind, TBinaryExpression>.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is not TBinaryExpression expression ||
                !IsCandidateExpression(expression))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c => ReplaceExpressionAsync(expression, root, context.Document),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        internal abstract bool IsCandidateExpression(TBinaryExpression expression);

        private Task<Document> ReplaceExpressionAsync(TBinaryExpression expression,
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
