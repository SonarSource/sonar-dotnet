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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class GenericTypeStrategyTest
{
    [TestMethod]
    public void SingleArgument_NoChangeStrategy_IsSupported()
    {
        var sut = new GenericTypeStrategy(typeof(List<int>), [new NoChangeStrategy(typeof(int))]);
        sut.IsSupported.Should().BeTrue();
        sut.Generate([]).Should().BeNull();
        sut.CompiletimeTypeSnippet.Should().Be("List<Int32>");
    }

    [TestMethod]
    public void SingleArgument_SkipStrategy_NotSupported()
    {
        var sut = new GenericTypeStrategy(typeof(List<Delegate>), [new SkipStrategy(typeof(Delegate))]);
        sut.IsSupported.Should().BeFalse();
        sut.Generate([]).Should().BeNull();
    }

    [TestMethod]
    public void SingleArgument_WrapStrategy_NotSupported()
    {
        var sut = new GenericTypeStrategy(typeof(List<FileScopedNamespaceDeclarationSyntax>), [new SyntaxNodeWrapStrategy(typeof(FileScopedNamespaceDeclarationSyntax), typeof(SyntaxNode), null, [])]);
        sut.IsSupported.Should().BeFalse();
        sut.Generate([]).Should().BeNull();
    }

    [TestMethod]
    public void MultipleArguments_AllSupportedStrategies_IsSupported()
    {
        var sut = new GenericTypeStrategy(typeof(Dictionary<int, string>), [new NoChangeStrategy(typeof(int)), new NoChangeStrategy(typeof(string))]);
        sut.IsSupported.Should().BeTrue();
        sut.Generate([]).Should().BeNull();
        sut.CompiletimeTypeSnippet.Should().Be("Dictionary<Int32, String>");
    }

    [TestMethod]
    public void MultipleArguments_OneUnsupportedStrategy_NotSupported()
    {
        var sut = new GenericTypeStrategy(typeof(Dictionary<int, string>), [new SkipStrategy(typeof(int)), new NoChangeStrategy(typeof(string))]);
        sut.IsSupported.Should().BeFalse();
        sut.Generate([]).Should().BeNull();
    }
}
