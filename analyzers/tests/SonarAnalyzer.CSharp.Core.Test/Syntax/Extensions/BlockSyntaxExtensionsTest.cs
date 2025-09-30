/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class BlockSyntaxExtensionsTest
{
    [TestMethod]
    public void IsEmpty_BlockMethodEmpty()
    {
        var declaration = MethodDeclarationForBlock(@"
{

}");
        declaration.Body.IsEmpty().Should().BeTrue();
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithCode()
    {
        var declaration = MethodDeclarationForBlock(@"
{
    var i = 0;
}");
        declaration.Body.IsEmpty().Should().BeFalse();
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
        declaration.Body.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithComment_Singleline_Off()
    {
        var declaration = MethodDeclarationForBlock(@"
{
    // Single line
}");
        declaration.Body.IsEmpty(treatCommentsAsContent: false).Should().BeTrue();
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithComment_Empty_On()
    {
        var declaration = MethodDeclarationForBlock(@"
{
    //
}");
        declaration.Body.IsEmpty(treatCommentsAsContent: true).Should().BeFalse(); // This is a questionable behavior.
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithComment_Multiline_On()
    {
        var declaration = MethodDeclarationForBlock(@"
{
    /* Multi line */
}");
        declaration.Body.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithComment_Multiline_Off()
    {
        var declaration = MethodDeclarationForBlock(@"
{
    /* Multi line */
}");
        declaration.Body.IsEmpty(treatCommentsAsContent: false).Should().BeTrue();
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
        declaration.Body.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeFalse();
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
        declaration.Body.IsEmpty(treatConditionalCompilationAsContent: false).Should().BeTrue();
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithConditionalCompilation_WithEmptyRegion_On()
    {
        var declaration = MethodDeclarationForBlock(@"
{
#region SomeRegion
#endregion
}");
        declaration.Body.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeTrue();
    }

    [TestMethod]
    public void IsEmpty_BlockMethodWithConditionalCompilation_WithEmptyConditional_On()
    {
        var declaration = MethodDeclarationForBlock(@"
{
#if SomeCondition
#endif
}");
        declaration.Body.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeTrue();
    }

    [TestMethod]
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
            declaration.Body.IsEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent).Should().BeTrue();
        }
        else
        {
            declaration.Body.IsEmpty(treatCommentsAsContent, treatConditionalCompilationAsContent).Should().BeFalse();
        }
    }

    [TestMethod]
    public void IsEmpty_TrailingTriviaOnOpenBrace_Comment()
    {
        var block = CreateBlock(afterOpen: CreateTriviaListWithComment());
        block.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
    }

    [TestMethod]
    public void IsEmpty_LeadingTriviaOnCloseBrace_Comment()
    {
        var block = CreateBlock(beforeClose: CreateTriviaListWithComment());
        block.IsEmpty(treatCommentsAsContent: true).Should().BeFalse();
    }

    [TestMethod]
    public void IsEmpty_TrailingTriviaOnOpenBrace_ConditionalCompilation()
    {
        var block = CreateBlock(afterOpen: CreateTriviaListWithConditionalCompilation());
        block.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeFalse();
    }

    [TestMethod]
    public void IsEmpty_LeadingTriviaOnCloseBrace_ConditionalCompilation()
    {
        var block = CreateBlock(beforeClose: CreateTriviaListWithConditionalCompilation());
        block.IsEmpty(treatConditionalCompilationAsContent: true).Should().BeFalse();
    }

    private static SyntaxTriviaList CreateTriviaListWithComment() =>
        TriviaList(Comment("// Comment"));

    private static SyntaxTriviaList CreateTriviaListWithConditionalCompilation() =>
        TriviaList(
            Trivia(IfDirectiveTrivia(IdentifierName("Something"), isActive: false, branchTaken: false, conditionValue: false)),
            DisabledText(@"var i= 0;"),
            Trivia(EndIfDirectiveTrivia(isActive: false)));

    private static BlockSyntax CreateBlock(SyntaxTriviaList beforeOpen = default,
                                           SyntaxTriviaList afterOpen = default,
                                           SyntaxTriviaList beforeClose = default,
                                           SyntaxTriviaList afterClose = default) =>
        Block(Token(beforeOpen, SyntaxKind.OpenBraceToken, afterOpen), statements: default, Token(beforeClose, SyntaxKind.CloseBraceToken, afterClose));

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
