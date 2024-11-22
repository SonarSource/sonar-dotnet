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

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.AnalysisContext;

public readonly struct SonarCodeFixContext
{
    private readonly CodeFixContext context;

    public readonly CancellationToken CancellationToken => context.CancellationToken;
    public readonly Document Document => context.Document;
    public readonly ImmutableArray<Diagnostic> Diagnostics => context.Diagnostics;
    public readonly TextSpan Span => context.Span;

    public SonarCodeFixContext(CodeFixContext context) =>
        this.context = context;

    public void RegisterCodeFix(string title, Func<CancellationToken, Task<Document>> createChangedDocument, ImmutableArray<Diagnostic> diagnostics) =>
        context.RegisterCodeFix(CodeAction.Create(title, createChangedDocument, title), diagnostics);

    public void RegisterCodeFix(string title, Func<CancellationToken, Task<Solution>> createChangedDocument, ImmutableArray<Diagnostic> diagnostics) =>
        context.RegisterCodeFix(CodeAction.Create(title, createChangedDocument, title), diagnostics);
}
