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

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class ArrayStrategyTest
{
    [TestMethod]
    public void CompiletimeType()
    {
        var sut = new ArrayStrategy(typeof(SyntaxNode[]), new ExtendStrategy(typeof(SyntaxNode), []));
        sut.IsSupported.Should().BeTrue();
        sut.ReturnTypeSnippet.Should().Be("SyntaxNode[]");
    }

    [TestMethod]
    public void WrappedType()
    {
        var sut = new ArrayStrategy(typeof(TupleTypeSyntax[]), new SyntaxNodeWrapStrategy(typeof(TupleTypeSyntax), typeof(CSharpSyntaxNode), null, []));
        sut.IsSupported.Should().BeTrue();
        sut.ReturnTypeSnippet.Should().Be("TupleTypeSyntaxWrapper[]");
    }

    [TestMethod]
    public void UnsupportedType()
    {
        var sut = new ArrayStrategy(typeof(Action[]), new SkipStrategy(typeof(Action)));
        sut.IsSupported.Should().BeFalse();
        sut.Invoking(x => x.ReturnTypeSnippet).Should().Throw<NotSupportedException>();
        sut.Invoking(x => x.CompiletimeTypeSnippet).Should().Throw<NotSupportedException>();
        sut.Invoking(x => x.ToConversionSnippet("from")).Should().Throw<NotSupportedException>();
    }
}
