/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.AnalysisContext;
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
