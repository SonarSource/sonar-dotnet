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

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantToArrayCallCodeFix : SonarCodeFix
    {
        private const string Title = "Remove redundant 'ToArray' call";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantToArrayCall.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var simpleNameSyntax = root.FindNode(diagnosticSpan) as SimpleNameSyntax;
            var memberAccessExpressionSyntax = simpleNameSyntax?.Parent as MemberAccessExpressionSyntax;

            if (memberAccessExpressionSyntax?.Parent is not InvocationExpressionSyntax invocationExpressionSyntax)
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.ReplaceNode(invocationExpressionSyntax, memberAccessExpressionSyntax.Expression);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
