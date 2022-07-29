/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class BlockSyntaxExtensionsTests
    {
        [TestMethod]
        public void IsEmpty_BlockMethodEmpty()
        {
            var declaration = MethodDeclarationForBlock(@"
{

}");
            declaration.Body.Should().BeEmpty();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithCode()
        {
            var declaration = MethodDeclarationForBlock(@"
{
    var i = 0;
}");
            declaration.Body.Should().BeNotEmpty();
        }

        [TestMethod]
        public void IsEmpty_ArgumentExceptionOnNull()
        {
            var code = @"
public class C
{
    public int M() => 1;
}";

            var declaration = MethodDeclaration(code);
            declaration.Body.Should().BeNull();
            var isEmpty = () => declaration.Body.IsEmpty();
            isEmpty.Should().Throw<ArgumentNullException>().Which.Message.Should().Contain("Value cannot be null").And.Contain("block");
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Singleline_On()
        {
            var declaration = MethodDeclarationForBlock(@"
{
    // Single line
}");
            declaration.Body.Should().BeNotEmpty(treatCommentsAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Singleline_Off()
        {
            var declaration = MethodDeclarationForBlock(@"
{
    // Single line
}");
            declaration.Body.Should().BeEmpty(treatCommentsAsContent: false);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Empty_On()
        {
            var declaration = MethodDeclarationForBlock(@"
{
    //
}");
            declaration.Body.Should().BeNotEmpty(treatCommentsAsContent: true); // This is a questionable behavior.
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Multiline_On()
        {
            var declaration = MethodDeclarationForBlock(@"
{
    /* Multi line */
}");
            declaration.Body.Should().BeNotEmpty(treatCommentsAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Multiline_Off()
        {
            var declaration = MethodDeclarationForBlock(@"
{
    /* Multi line */
}");
            declaration.Body.Should().BeEmpty(treatCommentsAsContent: false);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithDisabledCode_On()
        {
            var declaration = MethodDeclarationForBlock(@"
{
#if SomeThing
    var i = 0;
#endif
}");
            declaration.Body.Should().BeNotEmpty(treatConditionalCompilationAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithDisabledCode_Off()
        {
            var declaration = MethodDeclarationForBlock(@"
{
#if SomeThing
    var i = 0;
#endif
}");
            declaration.Body.Should().BeEmpty(treatConditionalCompilationAsContent: false);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithEmptyRegion_On()
        {
            var declaration = MethodDeclarationForBlock(@"
{
#region SomeRegion
#endregion
}");
            declaration.Body.Should().BeEmpty(treatConditionalCompilationAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithEmptyConditional_On()
        {
            var declaration = MethodDeclarationForBlock(@"
{
#if SomeCondition
#endif
}");
            declaration.Body.Should().BeEmpty(treatConditionalCompilationAsContent: true);
        }

        [DataTestMethod]
        [DataRow(true, true, false)]
        [DataRow(false, true, false)] // isEmpty should be true here, because the content of the conditional is just a comment and a comment should not be treated as content.
        [DataRow(true, false, true)]
        [DataRow(false, false, true)]
        public void IsEmpty_BlockMethodCombinations_CommentInConditional(bool treatCommentsAsContent, bool treatConditionalCompilationAsContent, bool isEmpty)
        {
            var declaration = MethodDeclarationForBlock(@"
{
#if SomeCondition
    // Some comment
#endif
}");
            if (isEmpty)
            {
                declaration.Body.Should().BeEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent);
            }
            else
            {
                declaration.Body.Should().BeNotEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent);
            }
        }

        [TestMethod]
        public void IsEmpty_TrailingTriviaOnOpenBrace_Comment()
        {
            var block = BlockWithoutTrivia();
            block = block.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(ParseTrailingTrivia("// Comment")));
            block.Should().BeNotEmpty(treatCommentsAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_LeadingTriviaOnCloseBrace_Comment()
        {
            var block = BlockWithoutTrivia();
            block = block.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(ParseLeadingTrivia("// Comment")));
            block.Should().BeNotEmpty(treatCommentsAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_TrailingTriviaOnOpenBrace_ConditionalCompilation()
        {
            var block = BlockWithoutTrivia();
            block = block.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(ParseTrailingTrivia("#if Something\r\n var i = 1; \r\n #endif")));
            block.Should().BeNotEmpty(treatConditionalCompilationAsContent: true);
        }

        [TestMethod]
        public void IsEmpty_LeadingTriviaOnCloseBrace_ConditionalCompilation()
        {
            var block = BlockWithoutTrivia();
            block = block.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(ParseLeadingTrivia("#if Something\r\n var i = 1; \r\n #endif")));
            block.Should().BeNotEmpty(treatConditionalCompilationAsContent: true);
        }

        private static BlockSyntax BlockWithoutTrivia()
        {
            var block = MethodDeclarationForBlock("{}").Body;
            block = block
                .WithOpenBraceToken(block.OpenBraceToken.WithoutTrivia())
                .WithCloseBraceToken(block.CloseBraceToken.WithoutTrivia());
            return block;
        }

        private static MethodDeclarationSyntax MethodDeclarationForBlock(string methodBlock)
            => MethodDeclaration(WrapInClass(methodBlock));

        private static string WrapInClass(string methodBlockOrArrow) => $@"
public class C
{{
    public void M()
    {methodBlockOrArrow}
}}";

        private static MethodDeclarationSyntax MethodDeclaration(string source)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetRoot();
            root.ContainsDiagnostics.Should().BeFalse();
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        }
    }

    internal static class BlockSyntaxExtensions
    {
        public class BlockSyntaxAssertions : ReferenceTypeAssertions<BlockSyntax, BlockSyntaxAssertions>
        {
            public BlockSyntaxAssertions(BlockSyntax block) : base(block)
            {
            }

            protected override string Identifier => "BlockSyntax";

            public AndConstraint<BlockSyntaxAssertions> BeEmpty(bool treatCommentsAsContent = true, bool treatConditionalCompilationAsContent = true, string because = "", params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() => Subject.IsEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent))
                    .ForCondition(result => result)
                    .FailWith("Expected block.IsEmpty() to return true{reason}, but false was returned.")
                    .Then
                    .Given(_ => Subject.IsNotEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent))
                    .ForCondition(result => !result)
                    .FailWith("Expected block.IsNotEmpty() to return false{reason}, but true was returned.");
                return new AndConstraint<BlockSyntaxAssertions>(this);
            }

            public AndConstraint<BlockSyntaxAssertions> BeNotEmpty(bool treatCommentsAsContent = true,
                                                                   bool treatConditionalCompilationAsContent = true,
                                                                   string because = "",
                                                                   params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() => Subject.IsEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent))
                    .ForCondition(result => !result)
                    .FailWith("Expected block.IsEmpty() to return false{reason}, but true was returned.")
                    .Then
                    .Given(_ => Subject.IsNotEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent))
                    .ForCondition(result => result)
                    .FailWith("Expected block.IsNotEmpty() to return true{reason}, but false was returned.");
                return new AndConstraint<BlockSyntaxAssertions>(this);
            }
        }

        public static BlockSyntaxAssertions Should(this BlockSyntax block)
            => new(block);
    }
}
