/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class FileScopedNamespaceDeclarationSyntaxWrapperTest
{
    [TestMethod]
    public void Equals_SameType_SameInstance_ReturnsTrue()
    {
        var syntax = FileScopedNamespace("Sample");
        var a = FileScopedNamespaceDeclarationSyntaxWrapper.From(syntax);
        var b = FileScopedNamespaceDeclarationSyntaxWrapper.From(syntax);

        a.Equals(b).Should().BeTrue();
        b.Equals(a).Should().BeTrue();
        Equals(a, b).Should().BeTrue();
        Equals(b, a).Should().BeTrue();
        (a == b).Should().BeTrue();
        (b == a).Should().BeTrue();
        (a != b).Should().BeFalse();
        (b != a).Should().BeFalse();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [TestMethod]
    public void Equals_SameType_DifferentInstance_ReturnsFalse()
    {
        var a = FileScopedNamespaceDeclarationSyntaxWrapper.From(FileScopedNamespace("A"));
        var b = FileScopedNamespaceDeclarationSyntaxWrapper.From(FileScopedNamespace("B"));

        a.Equals(b).Should().BeFalse();
        b.Equals(a).Should().BeFalse();
        Equals(a, b).Should().BeFalse();
        Equals(b, a).Should().BeFalse();
        (a == b).Should().BeFalse();
        (b == a).Should().BeFalse();
        (a != b).Should().BeTrue();
        (b != a).Should().BeTrue();
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [TestMethod]
    public void Equals_CrossType_SameInstance_IsTrue()
    {
        var syntax = FileScopedNamespace("Sample");
        var fileScoped = FileScopedNamespaceDeclarationSyntaxWrapper.From(syntax);
        var baseNamespace = BaseNamespaceDeclarationSyntaxWrapper.From(syntax);

        fileScoped.Equals(baseNamespace).Should().BeTrue();
        baseNamespace.Equals(fileScoped).Should().BeTrue();
        Equals(fileScoped, baseNamespace).Should().BeTrue();
        Equals(baseNamespace, fileScoped).Should().BeTrue();
        (fileScoped == baseNamespace).Should().BeTrue();
        (baseNamespace == fileScoped).Should().BeTrue();
        (fileScoped != baseNamespace).Should().BeFalse();
        (baseNamespace != fileScoped).Should().BeFalse();
        fileScoped.GetHashCode().Should().Be(baseNamespace.GetHashCode());
    }

    [TestMethod]
    public void Equals_CrossType_DifferentInstance_ReturnsFalse()
    {
        var fileScoped = FileScopedNamespaceDeclarationSyntaxWrapper.From(FileScopedNamespace("FileScoped"));
        var baseNamespace = BaseNamespaceDeclarationSyntaxWrapper.From(FileScopedNamespace("BaseNamespace"));

        fileScoped.Equals(baseNamespace).Should().BeFalse();
        baseNamespace.Equals(fileScoped).Should().BeFalse();
        Equals(fileScoped, baseNamespace).Should().BeFalse();
        Equals(baseNamespace, fileScoped).Should().BeFalse();
        (fileScoped == baseNamespace).Should().BeFalse();
        (baseNamespace == fileScoped).Should().BeFalse();
        (fileScoped != baseNamespace).Should().BeTrue();
        (baseNamespace != fileScoped).Should().BeTrue();
        fileScoped.GetHashCode().Should().NotBe(baseNamespace.GetHashCode());
    }

    [TestMethod]
    public void Equals_UnderlyingSyntaxToken_ReturnsTrue()
    {
        var syntax = FileScopedNamespace("Sample");
        var wrapper = FileScopedNamespaceDeclarationSyntaxWrapper.From(syntax);

        wrapper.Equals(syntax).Should().BeTrue();
        syntax.Equals(wrapper).Should().BeFalse();  // unavoidable
        Equals(wrapper, syntax).Should().BeTrue();
        Equals(syntax, wrapper).Should().BeFalse(); // unavoidable
    }

    [TestMethod]
    public void Equals_OtherSyntaxToken_ReturnsFalse()
    {
        var a = FileScopedNamespaceDeclarationSyntaxWrapper.From(FileScopedNamespace("A"));
        var b = FileScopedNamespaceDeclarationSyntaxWrapper.From(FileScopedNamespace("B"));

        a.Equals(b).Should().BeFalse();
        b.Equals(a).Should().BeFalse();
        Equals(a, b).Should().BeFalse();
        Equals(b, a).Should().BeFalse();
    }

    private static FileScopedNamespaceDeclarationSyntax FileScopedNamespace(string name) =>
        SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.IdentifierName(name));
}
