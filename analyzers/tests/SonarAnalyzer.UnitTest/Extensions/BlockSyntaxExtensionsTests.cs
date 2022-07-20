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
            var block = @"
{

}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty().Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithCode()
        {
            var block = @"
{
    var i = 0;
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty().Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_ArgumentExceptionOnNull()
        {
            var code = @"
public class C
{
    public int M() => 1;
}
";

            var declaration = MethodDeclaration(code);
            declaration.Body.Should().BeNull();
            var isEmpty = () => declaration.Body.IsEmpty();
            isEmpty.Should().Throw<ArgumentNullException>().Which.Message.Should().Contain("Value cannot be null").And.Contain("block");
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Singleline_On()
        {
            var block = @"
{
    // Single line
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Singleline_Off()
        {
            var block = @"
{
    // Single line
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatCommentsAsContent: false).Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Empty_On()
        {
            var block = @"
{
    //
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatCommentsAsContent: true).Should().BeFalse(); // This is a questionable behavior.
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Multiline_On()
        {
            var block = @"
{
    /* Multi line */
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithComment_Multiline_Off()
        {
            var block = @"
{
    /* Multi line */
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatCommentsAsContent: false).Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithDisabledCode_On()
        {
            var block = @"
{
#if SomeThing
    var i = 0;
#endif
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithDisabledCode_Off()
        {
            var block = @"
{
#if SomeThing
    var i = 0;
#endif
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatConditionalCompilationAsContent: false).Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithEmptyRegion_On()
        {
            var block = @"
{
#region SomeRegion
#endregion
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_BlockMethodWithConditionalCompilation_WithEmptyConditional_On()
        {
            var block = @"
{
#if SomeCondition
#endif
}
";
            var declaration = MethodDeclarationForBlock(block);
            declaration.Body.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(true, true, false)]
        [DataRow(false, true, false)] // isEmpty should be true here, because the content of the conditional is just a comment and a comment should not be treated as content.
        [DataRow(true, false, true)]
        [DataRow(false, false, true)]
        public void IsEmpty_BlockMethodCombinations_CommentInConditional(bool treatCommentsAsContent, bool treatConditionalCompilationAsContent, bool isEmpty)
        {
            var block = @"
{
#if SomeCondition
    // Some comment
#endif
}
";
            var declaration = MethodDeclarationForBlock(block);
            var result = declaration.Body.IsEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent);
            if (isEmpty)
            {
                result.Should().BeTrue();
            }
            else
            {
                result.Should().BeFalse();
            }
        }

        [TestMethod]
        public void IsEmpty_TrailingTriviaOnOpenBrace_Comment()
        {
            var block = BlockWithoutTrivia();
            block = block.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(ParseTrailingTrivia("// Comment")));
            block.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_LeadingTriviaOnCloseBrace_Comment()
        {
            var block = BlockWithoutTrivia();
            block = block.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(ParseLeadingTrivia("// Comment")));
            block.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_TrailingTriviaOnOpenBrace_ConditionalCompilation()
        {
            var block = BlockWithoutTrivia();
            block = block.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(ParseTrailingTrivia("#if Something\r\n var i = 1; \r\n #endif")));
            block.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeFalse();
        }

        [TestMethod]
        public void IsEmpty_LeadingTriviaOnCloseBrace_ConditionalCompilation()
        {
            var block = BlockWithoutTrivia();
            block = block.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(ParseLeadingTrivia("#if Something\r\n var i = 1; \r\n #endif")));
            block.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeFalse();
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
}
