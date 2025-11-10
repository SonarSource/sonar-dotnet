/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.CSharp;
using static SonarAnalyzer.CSharp.Core.Syntax.Extensions.SyntaxTokenExtensionsShared;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class SyntaxTokenExtensionsSharedTest
{
    [TestMethod]
    public void GetBindableParent_ForEmptyToken_ReturnsNull()
    {
        SyntaxToken empty = default;

        empty.GetBindableParent().Should().BeNull();
    }

    [TestMethod]
    public void GetBindableParent_ForInterpolatedStringTextTokenInInterpolatedStringTextToken_ReturnsInterpolatedStringExpression()
    {
        const string code = @"string x = $""a"";";

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.InterpolatedStringTextToken);

        var parent = aToken.GetBindableParent();
        parent.Kind().Should().Be(SyntaxKind.InterpolatedStringExpression);
    }

    [TestMethod]
    public void GetBindableParent_ForOpenBraceInsideInterpolatedStringTextToken_ReturnsInterpolation()
    {
        const string code = @"string x = $""{1}"";";

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.OpenBraceToken);

        var parent = aToken.GetBindableParent();
        parent.Kind().Should().Be(SyntaxKind.Interpolation);
    }

    [TestMethod]
    public void GetBindableParent_ForMemberAccessExpressionSyntax_ReturnsTheExpression()
    {
        const string code = @"this.Value;";

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var thisToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.ThisKeyword);

        var parent = thisToken.GetBindableParent();
        parent.Kind().Should().Be(SyntaxKind.ThisExpression);
    }

    [TestMethod]
    public void GetBindableParent_ForObjectCreationExpressionSyntax_ReturnsArgumentList()
    {
        const string code = @"var s = new string();";

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var openParentToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.OpenParenToken);

        var parent = openParentToken.GetBindableParent();
        parent.Kind().Should().Be(SyntaxKind.ArgumentList);
    }

    private static SyntaxToken GetFirstTokenOfKind(SyntaxTree syntaxTree, SyntaxKind kind) =>
        syntaxTree.GetRoot().DescendantTokens().First(token => token.IsKind(kind));
}
