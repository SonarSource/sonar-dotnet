/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CatchRethrowCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove redundant catch";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CatchRethrow.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            if (!(syntaxNode.Parent is TryStatementSyntax tryStatement))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = CalculateNewRoot(root, syntaxNode, tryStatement);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode currentNode, TryStatementSyntax tryStatement)
        {
            var isTryRemovable = tryStatement.Catches.Count == 1 && tryStatement.Finally == null;

            return isTryRemovable
                ? root.ReplaceNode(
                    tryStatement,
                    tryStatement.Block.Statements.Select(st => st.WithAdditionalAnnotations(Formatter.Annotation)))
                : root.RemoveNode(currentNode, SyntaxRemoveOptions.KeepNoTrivia);
        }
    }
}
