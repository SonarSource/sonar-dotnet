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
    public void IsSupported_NoChangeStrategy_True()
    {
        var sut = new GenericTypeStrategy(typeof(List<int>), [new NoChangeStrategy(typeof(int))]);
        sut.Generate([]).Should().BeNull();
        sut.IsSupported.Should().BeTrue();
    }

    [TestMethod]
    public void IsSupported_SkipStrategy_False()
    {
        var sut = new GenericTypeStrategy(typeof(List<Delegate>), [new SkipStrategy(typeof(Delegate))]);
        sut.Generate([]).Should().BeNull();
        sut.IsSupported.Should().BeFalse();
    }

    [TestMethod]
    public void IsSupported_WrapStrategy_False()
    {
        var sut = new GenericTypeStrategy(typeof(List<FileScopedNamespaceDeclarationSyntax>), [new SyntaxNodeWrapStrategy(typeof(FileScopedNamespaceDeclarationSyntax), typeof(SyntaxNode), [])]);
        sut.Generate([]).Should().BeNull();
        sut.IsSupported.Should().BeFalse();
    }
}
