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
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CSharp.Syntax.Extensions;
using SonarAnalyzer.CSharp.Syntax.Utilities;

namespace SonarAnalyzer.Test.Syntax.Extensions;

[TestClass]
public class SyntaxTreeExtensionsTest
{
    [TestMethod]
    public void ConditionalCompilationRegions_NoDirectives() =>
        Parse("class Sample { }").ConditionalCompilationRegions.Should().BeEmpty();

    [TestMethod]
    public void ConditionalCompilationRegions_NoConditionalDirectives() =>
        Parse("""
            #region Name
            #pragma warning disable CS0168
            class Sample { }
            #endregion
            """).ConditionalCompilationRegions.Should().BeEmpty();

    [TestMethod]
    public void ConditionalCompilationRegions_If_Inactive()
    {
        var tree = Parse("""
            #if DEBUG
            class Sample { }
            #endif
            """);
        var region = tree.ConditionalCompilationRegions.Should().ContainSingle().Subject;
        region.Range.Start.Should().BeOfType<IfDirectiveTriviaSyntax>();
        region.Range.End.Should().BeOfType<EndIfDirectiveTriviaSyntax>();
        Text(tree, region.Range.Span).Should().StartWith("#if DEBUG").And.EndWith("#endif");
        region.HidesContent.Should().BeTrue();
        DisabledTexts(tree, region).Should().Equal("class Sample { }");
        region.Branches.Select(x => x.IsActive).Should().Equal(false);
    }

    [TestMethod]
    public void ConditionalCompilationRegions_If_Active()
    {
        const string source = """
            #if DEBUG
            class Sample { }
            #endif
            """;
        var region = Parse(source, "DEBUG").ConditionalCompilationRegions.Should().ContainSingle().Subject;
        region.HidesContent.Should().BeFalse();
        region.DisabledSpans.Should().BeEmpty();
        region.Branches.Select(x => x.IsActive).Should().Equal(true);
    }

    [TestMethod]
    public void ConditionalCompilationRegions_IfElse()
    {
        var tree = Parse("""
            #if DEBUG
            class Debug { }
            #else
            class Release { }
            #endif
            """);
        var region = tree.ConditionalCompilationRegions.Should().ContainSingle().Subject;
        region.HidesContent.Should().BeTrue();
        DisabledTexts(tree, region).Should().Equal("class Debug { }");
        region.Branches.Select(x => x.Range.Start.Kind()).Should().Equal(SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElseDirectiveTrivia);
        region.Branches.Select(x => x.Range.End.Kind()).Should().Equal(SyntaxKind.ElseDirectiveTrivia, SyntaxKind.EndIfDirectiveTrivia);
        region.Branches.Select(x => x.IsActive).Should().Equal(false, true);
    }

    [TestMethod]
    public void ConditionalCompilationRegions_IfElifElse()
    {
        const string source = """
            #if FIRST
            class First { }
            #elif SECOND
            class Second { }
            #else
            class Third { }
            #endif
            """;
        var tree = Parse(source, "SECOND");
        var region = tree.ConditionalCompilationRegions.Should().ContainSingle().Subject;
        DisabledTexts(tree, region).Should().Equal("class First { }", "class Third { }");
        region.Branches.Select(x => x.Range.Start.Kind()).Should().Equal(SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElifDirectiveTrivia, SyntaxKind.ElseDirectiveTrivia);
        region.Branches.Select(x => x.IsActive).Should().Equal(false, true, false);
    }

    [TestMethod]
    public void ConditionalCompilationRegions_EmptyInactiveBranch_HidesNothing()
    {
        var region = Parse("""
            #if DEBUG
            #else
            class Release { }
            #endif
            """).ConditionalCompilationRegions.Should().ContainSingle().Subject;
        region.HidesContent.Should().BeFalse();
        region.Branches.Select(x => x.IsActive).Should().Equal(false, true);
    }

    [TestMethod]
    public void ConditionalCompilationRegions_Nested()
    {
        var tree = Parse("""
            #if true
            class Outer
            {
            #if DEBUG
                void Method() { }
            #endif
            }
            #endif
            """);
        var regions = tree.ConditionalCompilationRegions;
        regions.Should().HaveCount(2);
        regions[0].Range.Span.Contains(regions[1].Range.Span).Should().BeTrue();
        regions.Select(x => x.Branches.Single().IsActive).Should().Equal(true, false);
        DisabledTexts(tree, regions[0]).Should().Equal("void Method() { }");
        DisabledTexts(tree, regions[1]).Should().Equal("void Method() { }");
    }

    [TestMethod]
    public void ConditionalCompilationRegions_NestedInsideInactiveBranch()
    {
        const string source = """
            #if DEBUG
            #if TRACE
            class Inner { }
            #endif
            #endif
            """;
        var tree = Parse(source, "TRACE");
        var regions = tree.ConditionalCompilationRegions;
        regions.Should().HaveCount(2);
        regions.Select(x => x.Branches.Single().IsActive).Should().Equal(false, false);
        regions.Should().AllSatisfy(x => DisabledTexts(tree, x).Should().Equal("class Inner { }"));
    }

    [TestMethod]
    public void ConditionalCompilationRegions_MissingEndIf()
    {
        var region = Parse("""
            #if true
            class Sample { }
            """).ConditionalCompilationRegions.Should().ContainSingle().Subject;
        region.Range.End.Should().Be(region.Range.Start);
        region.Branches.Should().BeEmpty();
    }

    [TestMethod]
    public void ConditionalCompilationRegions_MissingIf() =>
        Parse("""
            class Sample { }
            #endif
            """).ConditionalCompilationRegions.Should().BeEmpty();

    [TestMethod]
    public void ConditionalCompilationRegions_IsCached()
    {
        var tree = Parse("""
            #if DEBUG
            #endif
            """);
        tree.ConditionalCompilationRegions.Should().BeSameAs(tree.ConditionalCompilationRegions);
    }

    private static SyntaxTree Parse(string source, params string[] preprocessorSymbols) =>
        CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(preprocessorSymbols: preprocessorSymbols));

    private static string Text(SyntaxTree tree, TextSpan span) =>
        tree.GetText().ToString(span);

    private static IEnumerable<string> DisabledTexts(SyntaxTree tree, ConditionalCompilationRegion region) =>
        region.DisabledSpans.Select(x => Text(tree, x).Trim());
}
