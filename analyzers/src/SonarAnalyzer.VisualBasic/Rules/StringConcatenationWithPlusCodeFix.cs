﻿/*
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
    public sealed class StringConcatenationWithPlusCodeFix : SonarCodeFix
    {
        internal const string Title = "Change to '&'";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(StringConcatenationWithPlus.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is BinaryExpressionSyntax binary)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = CalculateNewRoot(root, binary);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private static SyntaxNode CalculateNewRoot(SyntaxNode root, BinaryExpressionSyntax currentAsBinary)
        {
            return root.ReplaceNode(currentAsBinary,
                SyntaxFactory.ConcatenateExpression(
                    currentAsBinary.Left,
                    SyntaxFactory.Token(SyntaxKind.AmpersandToken).WithTriviaFrom(currentAsBinary.OperatorToken),
                    currentAsBinary.Right));
        }
    }
}
