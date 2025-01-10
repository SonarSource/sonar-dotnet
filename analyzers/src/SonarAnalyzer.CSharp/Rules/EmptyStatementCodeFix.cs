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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class EmptyStatementCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove empty statement";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EmptyStatement.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            if (!IsInBlock(syntaxNode))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = RemoveUnusedCode(root, syntaxNode);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static bool IsInBlock(SyntaxNode node)
        {
            return node.Parent is BlockSyntax;
        }

        internal static SyntaxNode RemoveUnusedCode(SyntaxNode root, SyntaxNode syntaxNode)
        {
            return root.RemoveNode(syntaxNode, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepEndOfLine);
        }
    }
}
