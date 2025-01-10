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

using ISymbolExtensionsVB = SonarAnalyzer.VisualBasic.Core.Extensions.ISymbolExtensions;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class ISymbolExtensionsTest
{
    [TestMethod]
    public void GetDescendantNodes_ForNullSourceTree_ReturnsEmpty() =>
        ISymbolExtensionsVB.GetDescendantNodes(Location.None, SyntaxFactory.ModifiedIdentifier("a")).Should().BeEmpty();

    [TestMethod]
    public void GetDescendantNodes_ForDifferentSyntaxTrees_ReturnsEmpty()
    {
        var first = SyntaxFactory.ParseSyntaxTree("Dim a As String");
        var identifier = first.Single<ModifiedIdentifierSyntax>();

        var second = SyntaxFactory.ParseSyntaxTree("Dim a As String");
        ISymbolExtensionsVB.GetDescendantNodes(identifier.GetLocation(), second.GetRoot()).Should().BeEmpty();
    }

    [TestMethod]
    public void GetDescendantNodes_ForMissingVariableDeclarator_ReturnsEmpty()
    {
        var tree = SyntaxFactory.ParseSyntaxTree(@"new FileSystemAccessRule(""User"", FileSystemRights.ListDirectory, AccessControlType.Allow)");
        ISymbolExtensionsVB.GetDescendantNodes(tree.GetRoot().GetLocation(), tree.GetRoot()).Should().BeEmpty();
    }
}
