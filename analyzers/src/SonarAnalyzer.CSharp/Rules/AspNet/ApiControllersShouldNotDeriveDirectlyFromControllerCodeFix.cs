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
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class ApiControllersShouldNotDeriveDirectlyFromControllerCodeFix : SonarCodeFix
{
    internal const string Title = "Inherit from ControllerBase instead of Controller.";

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ApiControllersShouldNotDeriveDirectlyFromController.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var toReplace = root.FindNode(context.Diagnostics.First().Location.SourceSpan) as BaseTypeSyntax;
        var replacement = root.ReplaceNode(toReplace, SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("ControllerBase").WithTriviaFrom(toReplace)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        context.RegisterCodeFix(
           Title,
           x => Task.FromResult(context.Document.WithSyntaxRoot(replacement)), context.Diagnostics);
        return Task.CompletedTask;
    }
}
