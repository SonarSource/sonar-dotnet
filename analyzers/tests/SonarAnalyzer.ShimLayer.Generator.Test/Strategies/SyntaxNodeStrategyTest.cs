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
public class SyntaxNodeStrategyTest
{
    [TestMethod]
    public void Generate_Empty()
    {
        var sut = new SyntaxNodeStrategy(
            typeof(RecordDeclarationSyntax),
            typeof(TypeDeclarationSyntax),
            []);
        var result = sut.Generate(new Dictionary<Type, Strategy>());
        result.Should().BeIgnoringLineEndings(
            """
            using System;
            using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Text;

            namespace SonarAnalyzer.ShimLayer;

            public readonly partial struct RecordDeclarationSyntaxWrapper: ISyntaxWrapper<TypeDeclarationSyntax>
            {
                public const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax";
                private static readonly Type WrappedType;

                private readonly TypeDeclarationSyntax node;

                static RecordDeclarationSyntaxWrapper()
                {
                    WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof(RecordDeclarationSyntaxWrapper));

                }

                private RecordDeclarationSyntaxWrapper(TypeDeclarationSyntax node) =>
                    this.node = node;

                [Obsolete]
                public TypeDeclarationSyntax SyntaxNode => this.node;


            }
            """);
    }

    [TestMethod]
    public void Generate_RecordDeclarationSyntax()
    {
        var sut = new SyntaxNodeStrategy(
            typeof(RecordDeclarationSyntax),
            typeof(TypeDeclarationSyntax),
            [
                new(typeof(RecordDeclarationSyntax).GetMember(nameof(SyntaxNode.Span))[0], true),
                new(typeof(RecordDeclarationSyntax).GetMember(nameof(RecordDeclarationSyntax.ClassOrStructKeyword))[0], false)
            ]);
        var result = sut.Generate(new Dictionary<Type, Strategy>());
        result.Should().BeIgnoringLineEndings(
            """
            using System;
            using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Text;

            namespace SonarAnalyzer.ShimLayer;

            public readonly partial struct RecordDeclarationSyntaxWrapper: ISyntaxWrapper<TypeDeclarationSyntax>
            {
                public const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax";
                private static readonly Type WrappedType;

                private readonly TypeDeclarationSyntax node;

                static RecordDeclarationSyntaxWrapper()
                {
                    WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof(RecordDeclarationSyntaxWrapper));
                    ClassOrStructKeywordAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TypeDeclarationSyntax, SyntaxToken>(WrappedType, nameof(ClassOrStructKeyword));
                }

                private RecordDeclarationSyntaxWrapper(TypeDeclarationSyntax node) =>
                    this.node = node;

                [Obsolete]
                public TypeDeclarationSyntax SyntaxNode => this.node;

                public TextSpan Span => this.node.Span;
                private static readonly Func<TypeDeclarationSyntax, SyntaxToken> ClassOrStructKeywordAccessor;
                public SyntaxToken ClassOrStructKeyword => ClassOrStructKeywordAccessor(this.node);
            }
            """);
    }
}
