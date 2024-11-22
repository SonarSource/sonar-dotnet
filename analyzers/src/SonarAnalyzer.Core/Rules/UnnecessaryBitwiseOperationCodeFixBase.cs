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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules
{
    public abstract class UnnecessaryBitwiseOperationCodeFixBase : SonarCodeFix
    {
        internal const string Title = "Remove bitwise operation";

        protected abstract Func<SyntaxNode> CreateNewRoot(SyntaxNode root, TextSpan diagnosticSpan, bool isReportingOnLeft);

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UnnecessaryBitwiseOperationBase.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var isReportingOnLeft = diagnostic.Properties.ContainsKey(UnnecessaryBitwiseOperationBase.IsReportingOnLeftKey)
                && bool.Parse(diagnostic.Properties[UnnecessaryBitwiseOperationBase.IsReportingOnLeftKey]);
            if (CreateNewRoot(root, diagnostic.Location.SourceSpan, isReportingOnLeft) is { }  createNewRoot)
            {
                context.RegisterCodeFix(Title, c => Task.FromResult(context.Document.WithSyntaxRoot(createNewRoot())), context.Diagnostics);
            }
            return Task.CompletedTask;
        }
    }
}
