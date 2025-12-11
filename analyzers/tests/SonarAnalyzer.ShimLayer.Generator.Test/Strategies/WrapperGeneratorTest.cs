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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class WrapperGeneratorTest
{
    [TestMethod]
    public void SkipStrategy()
    {
        var sut = new WrapperGenerator();
        sut.GenerateWrapper(typeof(ClassDeclarationSyntax), new() { { typeof(ClassDeclarationSyntax), new SkipStrategy() } }).Should().BeNull();
    }

    [TestMethod]
    public void SyntaxNodeStrategy()
    {
        var sut = new WrapperGenerator();
        var syntaxNodeStrategy = new SyntaxNodeStrategy(
            typeof(RecordDeclarationSyntax),
            typeof(TypeDeclarationSyntax),
            []);
        var result = sut.GenerateWrapper(typeof(RecordDeclarationSyntax), new() { { typeof(RecordDeclarationSyntax), syntaxNodeStrategy } });
        result.Name.Should().Be("RecordDeclarationSyntaxWrapper.g.cs");
        result.Content.Should().StartWith("using System;");
    }
}
