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
using SonarAnalyzer.Core.AnalysisContext;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class DummyCodeFixCS : DummyCodeFix
{
    protected override SyntaxNode NewNode() => CS.SyntaxFactory.LiteralExpression(CS.SyntaxKind.DefaultLiteralExpression);
}

[ExportCodeFixProvider(LanguageNames.VisualBasic)]
public class DummyCodeFixVB : DummyCodeFix
{
    protected override SyntaxNode NewNode() => VB.SyntaxFactory.NothingLiteralExpression(VB.SyntaxFactory.Token(VB.SyntaxKind.NothingKeyword));
}

public abstract class DummyCodeFix : SonarCodeFix
{
    protected abstract SyntaxNode NewNode();

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("SDummy");

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        context.RegisterCodeFix("Dummy Action", _ =>
        {
            var oldNode = root.FindNode(context.Diagnostics.Single().Location.SourceSpan);
            var newRoot = root.ReplaceNode(oldNode, NewNode().WithTriviaFrom(oldNode));
            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
        }, context.Diagnostics);
        return Task.CompletedTask;
    }
}

internal class DummyCodeFixNoAttribute : DummyCodeFix
{
    protected override SyntaxNode NewNode() => throw new NotSupportedException();
}
