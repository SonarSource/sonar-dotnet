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
using SonarAnalyzer.TestFramework.Extensions;

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class SyntaxNodeStrategyTests
{
    [TestMethod]
    public void SyntaxNodeStrategy_Empty()
    {
        var sut = new SyntaxNodeStrategy(
            typeof(RecordDeclarationSyntax),
            []);
        var result = sut.Generate(new Dictionary<Type, Strategy>());
        result.Should().BeIgnoringLineEndings(
            """
            namespace SonarAnalyzer.ShimLayer;

            public readonly struct RecordDeclarationSyntaxWrapper
            {
                private readonly Microsoft.CodeAnalysis.SyntaxNode _node;
                public RecordDeclarationSyntaxWrapper(Microsoft.CodeAnalysis.SyntaxNode node)
                {
                    _node = node;
                }
                public Microsoft.CodeAnalysis.SyntaxNode SyntaxNode => _node;

                
                
            }
            """);
    }

    [TestMethod]
    public void SyntaxNodeStrategy_PassThroughAndWrappedMembers()
    {
        var sut = new SyntaxNodeStrategy(
            typeof(RecordDeclarationSyntax),
            [
            new(typeof(RecordDeclarationSyntax).GetMember(nameof(SyntaxNode.Span))[0], false),
            new(typeof(RecordDeclarationSyntax).GetMember(nameof(RecordDeclarationSyntax.ClassOrStructKeyword))[0], true)
            ]);
        var result = sut.Generate(new Dictionary<Type, Strategy>());
        result.Should().BeIgnoringLineEndings(
            """
            namespace SonarAnalyzer.ShimLayer;

            public readonly struct RecordDeclarationSyntaxWrapper
            {
                private readonly Microsoft.CodeAnalysis.SyntaxNode _node;
                public RecordDeclarationSyntaxWrapper(Microsoft.CodeAnalysis.SyntaxNode node)
                {
                    _node = node;
                }
                public Microsoft.CodeAnalysis.SyntaxNode SyntaxNode => _node;

                public Wrap Span { get; set; }
                public PassThrough ClassOrStructKeyword { get; set; }
            }
            """);
    }
}
