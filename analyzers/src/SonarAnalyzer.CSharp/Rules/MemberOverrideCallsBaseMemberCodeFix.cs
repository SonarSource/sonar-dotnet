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
    public sealed class MemberOverrideCallsBaseMemberCodeFix : SonarCodeFix
    {
        private const string Title = "Remove redundant override";
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(MemberOverrideCallsBaseMember.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnosticSpan = context.Diagnostics.First().Location.SourceSpan;

            if (!(root.FindNode(diagnosticSpan) is MemberDeclarationSyntax member))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.RemoveNode(member, SyntaxRemoveOptions.KeepNoTrivia);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
