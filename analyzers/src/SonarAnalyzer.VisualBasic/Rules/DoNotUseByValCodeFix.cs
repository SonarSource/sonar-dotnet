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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class DoNotUseByValCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove the 'ByVal' modifier.";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DoNotUseByVal.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root.FindNode(diagnosticSpan) is ParameterSyntax oldNode)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var modifiers = default(SyntaxTokenList)
                            .AddRange(oldNode.Modifiers.Where(m => !DoNotUseByVal.IsByVal(m)));

                        var newNode = oldNode.WithModifiers(modifiers);
                        var newRoot = root.ReplaceNode(oldNode, newNode);

                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }
    }
}
