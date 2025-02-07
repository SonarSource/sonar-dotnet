/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public abstract class UseUnixEpochCodeFixBase<TSyntaxKind> : SonarCodeFix
        where TSyntaxKind : struct
    {
        internal const string Title = "Use UnixEpoch field";

        protected abstract SyntaxNode ReplaceConstructorWithField(SyntaxNode root, SyntaxNode node, SonarCodeFixContext context);

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(UseUnixEpochBase<TSyntaxKind>.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            context.RegisterCodeFix(
                Title,
                _ =>
                {
                    var newRoot = ReplaceConstructorWithField(root, node, context);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
