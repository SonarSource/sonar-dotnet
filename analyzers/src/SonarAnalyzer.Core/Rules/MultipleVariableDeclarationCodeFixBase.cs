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

namespace SonarAnalyzer.Rules;

public abstract class MultipleVariableDeclarationCodeFixBase : SonarCodeFix
{
    internal const string Title = "Separate declarations";

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MultipleVariableDeclarationConstants.DiagnosticId);

    protected sealed override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        context.RegisterCodeFix(
            Title,
            c =>
            {
                var newRoot = CalculateNewRoot(root, node);
                return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
            },
            context.Diagnostics);

        return Task.CompletedTask;
    }

    protected abstract SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node);
}
