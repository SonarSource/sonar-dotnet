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
        var result = sut.Generate([]);
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



                public static explicit operator RecordDeclarationSyntaxWrapper(SyntaxNode node)
                {
                    if (node is null)
                    {
                        return default;
                    }

                    if (!IsInstance(node))
                    {
                        throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                    }

                    return new RecordDeclarationSyntaxWrapper((TypeDeclarationSyntax)node);
                }

                public static implicit operator TypeDeclarationSyntax(RecordDeclarationSyntaxWrapper wrapper) =>
                    wrapper.node;

                public static bool IsInstance(SyntaxNode node) =>
                    node is not null && LightupHelpers.CanWrapNode(node, WrappedType);
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
        var result = sut.Generate([]);
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
                public SyntaxToken ClassOrStructKeyword => (SyntaxToken)ClassOrStructKeywordAccessor(this.node);

                public static explicit operator RecordDeclarationSyntaxWrapper(SyntaxNode node)
                {
                    if (node is null)
                    {
                        return default;
                    }

                    if (!IsInstance(node))
                    {
                        throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                    }

                    return new RecordDeclarationSyntaxWrapper((TypeDeclarationSyntax)node);
                }

                public static implicit operator TypeDeclarationSyntax(RecordDeclarationSyntaxWrapper wrapper) =>
                    wrapper.node;

                public static bool IsInstance(SyntaxNode node) =>
                    node is not null && LightupHelpers.CanWrapNode(node, WrappedType);
            }
            """);
    }

    [TestMethod]
    public void Generate_IsPatternSyntax()
    {
        var sut = new SyntaxNodeStrategy(
            typeof(IsPatternExpressionSyntax),
            typeof(ExpressionSyntax),
            [
                new(typeof(IsPatternExpressionSyntax).GetMember(nameof(IsPatternExpressionSyntax.Pattern))[0], false),
            ]);
        var patternSyntaxStrategy = new SyntaxNodeStrategy(typeof(PatternSyntax), typeof(CSharpSyntaxNode), []);

        var result = sut.Generate(new() { { typeof(PatternSyntax), patternSyntaxStrategy } });
        result.Should().BeIgnoringLineEndings(
            """
            using System;
            using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Text;

            namespace SonarAnalyzer.ShimLayer;

            public readonly partial struct IsPatternExpressionSyntaxWrapper: ISyntaxWrapper<ExpressionSyntax>
            {
                public const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.IsPatternExpressionSyntax";
                private static readonly Type WrappedType;

                private readonly ExpressionSyntax node;

                static IsPatternExpressionSyntaxWrapper()
                {
                    WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof(IsPatternExpressionSyntaxWrapper));
                    PatternAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<ExpressionSyntax, CSharpSyntaxNode>(WrappedType, nameof(Pattern));
                }

                private IsPatternExpressionSyntaxWrapper(ExpressionSyntax node) =>
                    this.node = node;

                [Obsolete]
                public ExpressionSyntax SyntaxNode => this.node;

                private static readonly Func<ExpressionSyntax, CSharpSyntaxNode> PatternAccessor;
                public PatternSyntaxWrapper Pattern => (PatternSyntaxWrapper)PatternAccessor(this.node);

                public static explicit operator IsPatternExpressionSyntaxWrapper(SyntaxNode node)
                {
                    if (node is null)
                    {
                        return default;
                    }

                    if (!IsInstance(node))
                    {
                        throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                    }

                    return new IsPatternExpressionSyntaxWrapper((ExpressionSyntax)node);
                }

                public static implicit operator ExpressionSyntax(IsPatternExpressionSyntaxWrapper wrapper) =>
                    wrapper.node;

                public static bool IsInstance(SyntaxNode node) =>
                    node is not null && LightupHelpers.CanWrapNode(node, WrappedType);
            }
            """);
    }

    [TestMethod]
    public void Generate_SkippedMembers_DoNotProduceEmptyLines()
    {
        var unsupportedMember = new MemberDescriptor(typeof(SyntaxNode).GetMembers().OfType<MethodInfo>().First(x => x.ReturnType.IsNested), true);
        var sut = new SyntaxNodeStrategy(
            typeof(SyntaxNode),
            typeof(SyntaxNode),
            Enumerable.Repeat(unsupportedMember, 20).ToList());    // This should not produce 20 empty lines

        var result = sut.Generate(new() { { typeof(PatternSyntax), sut } });
        result.Should().BeIgnoringLineEndings(
            """
            using System;
            using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Text;

            namespace SonarAnalyzer.ShimLayer;

            public readonly partial struct SyntaxNodeWrapper: ISyntaxWrapper<SyntaxNode>
            {
                public const string WrappedTypeName = "Microsoft.CodeAnalysis.SyntaxNode";
                private static readonly Type WrappedType;

                private readonly SyntaxNode node;

                static SyntaxNodeWrapper()
                {
                    WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof(SyntaxNodeWrapper));

                }

                private SyntaxNodeWrapper(SyntaxNode node) =>
                    this.node = node;

                [Obsolete]
                public SyntaxNode SyntaxNode => this.node;



                public static explicit operator SyntaxNodeWrapper(SyntaxNode node)
                {
                    if (node is null)
                    {
                        return default;
                    }

                    if (!IsInstance(node))
                    {
                        throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                    }

                    return new SyntaxNodeWrapper((SyntaxNode)node);
                }

                public static implicit operator SyntaxNode(SyntaxNodeWrapper wrapper) =>
                    wrapper.node;

                public static bool IsInstance(SyntaxNode node) =>
                    node is not null && LightupHelpers.CanWrapNode(node, WrappedType);
            }
            """);
    }

    [TestMethod]
    public void Generate_PropagateMemberAttributes()
    {
        var sut = new SyntaxNodeStrategy(
            typeof(IndexerDeclarationSyntax),
            typeof(SyntaxNode),
            [
                new(typeof(IndexerDeclarationSyntax).GetMember("Semicolon")[0], true),  // Has ObsoleteAttribute to render
                new(typeof(AliasQualifiedNameSyntax).GetMember("Parent")[0], true)      // Has NullableAttribute to ignore
            ]);
        var result = sut.Generate([]);
        result.Should().BeIgnoringLineEndings("""
            using System;
            using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Microsoft.CodeAnalysis.Text;

            namespace SonarAnalyzer.ShimLayer;

            public readonly partial struct IndexerDeclarationSyntaxWrapper: ISyntaxWrapper<SyntaxNode>
            {
                public const string WrappedTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.IndexerDeclarationSyntax";
                private static readonly Type WrappedType;

                private readonly SyntaxNode node;

                static IndexerDeclarationSyntaxWrapper()
                {
                    WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof(IndexerDeclarationSyntaxWrapper));

                }

                private IndexerDeclarationSyntaxWrapper(SyntaxNode node) =>
                    this.node = node;

                [Obsolete]
                public SyntaxNode SyntaxNode => this.node;

                [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
                [System.ObsoleteAttribute("This member is obsolete.", true)]
                public SyntaxToken Semicolon => this.node.Semicolon;
                public SyntaxNode Parent => this.node.Parent;

                public static explicit operator IndexerDeclarationSyntaxWrapper(SyntaxNode node)
                {
                    if (node is null)
                    {
                        return default;
                    }

                    if (!IsInstance(node))
                    {
                        throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                    }

                    return new IndexerDeclarationSyntaxWrapper((SyntaxNode)node);
                }

                public static implicit operator SyntaxNode(IndexerDeclarationSyntaxWrapper wrapper) =>
                    wrapper.node;

                public static bool IsInstance(SyntaxNode node) =>
                    node is not null && LightupHelpers.CanWrapNode(node, WrappedType);
            }
            """);
    }
}
