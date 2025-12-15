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

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class SkipStrategyTest
{
    [TestMethod]
    public void SkipStrategy()
    {
        var sut = new SkipStrategy(typeof(SyntaxNode));
        sut.Generate([]).Should().BeNull();
    }

    [TestMethod]
    public void ReturnTypeSnippet() =>
        new SkipStrategy(typeof(SyntaxNode)).Invoking(x => x.ReturnTypeSnippet()).Should().Throw<NotSupportedException>();

    [TestMethod]
    public void ToConversionSnippet() =>
        new SkipStrategy(typeof(SyntaxNode)).Invoking(x => x.ToConversionSnippet("Lorem ipsum")).Should().Throw<NotSupportedException>();
}
