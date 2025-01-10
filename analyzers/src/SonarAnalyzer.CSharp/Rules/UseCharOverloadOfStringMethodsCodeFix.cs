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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class UseCharOverloadOfStringMethodsCodeFix : SonarCodeFix
{
    private const string Title = "Convert to char.";
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseCharOverloadOfStringMethods.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        if (root.FindNode(diagnostic.Location.SourceSpan) is { Parent: { Parent: InvocationExpressionSyntax invocation } }
            && invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax node)
        {
            context.RegisterCodeFix(
                Title,
                c => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, Convert(node)))),
                context.Diagnostics);
        }
        return Task.CompletedTask;
    }

    private static LiteralExpressionSyntax Convert(LiteralExpressionSyntax node) =>
        LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal(node.Token.ValueText[0]));
}
