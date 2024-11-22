/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class ArrayCreationLongSyntaxCodeFix : SonarCodeFix
    {
        internal const string Title = "Use an array literal";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ArrayCreationLongSyntax.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan);

            ArrayCreationExpressionSyntax arrayCreation;
            switch (node.Kind())
            {
                case SyntaxKind.SimpleArgument:
                    arrayCreation = ((SimpleArgumentSyntax)node).Expression as ArrayCreationExpressionSyntax;
                    break;

                case SyntaxKind.ArrayCreationExpression:
                    arrayCreation = (ArrayCreationExpressionSyntax)node;
                    break;

                default:
                    arrayCreation = null;
                    break;
            }

            if (arrayCreation != null)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            arrayCreation,
                            arrayCreation.Initializer);

                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }
    }
}
