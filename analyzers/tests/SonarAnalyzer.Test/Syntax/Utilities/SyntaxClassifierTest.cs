/*
 * Copyright (C) 2015-2024 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using SonarAnalyzer.CSharp.Core.Syntax.Utilities;
using SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
using SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;
using CS = Microsoft.CodeAnalysis.CSharp;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxVB = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class SyntaxClassifierTest
{
    [DataTestMethod]
    [DataRow("while (condition) { }")]
    [DataRow("while (a && (b || !condition)) { }")]
    [DataRow("do { } while (condition);")]
    [DataRow("do { } while (a && (b || !condition));")]
    [DataRow("for(; condition; ) { }")]
    [DataRow("for(; a && (b || !condition); ) { }")]
    public void IsInLoopCondition_Loops_CS(string code) =>
        IsInLoopConditionCS(code).Should().BeTrue();

    [TestMethod]
    public void IsInLoopCondition_If_CS()
    {
        const string code = """
            while (a)
            {
                do
                {
                    if (condition)  // This is asserted
                    {
                    }
                }
                while (b);
            }
            """;
        IsInLoopConditionCS(code).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("While Condition : End While")]
    [DataRow("While A AndAlso (B OrElse Not Condition) : End While")]
    [DataRow("Do While Condition : Loop")]
    [DataRow("Do While A AndAlso (B OrElse Not Condition) : Loop")]
    [DataRow("Do : Loop While Condition")]
    [DataRow("Do : Loop While Condition")]
    [DataRow("Do : Loop While A AndAlso (B OrElse Not Condition)")]
    [DataRow("Do : Loop Until Condition")]
    [DataRow("Do : Loop Until Condition")]
    [DataRow("Do : Loop Until A AndAlso (B OrElse Not Condition)")]
    [DataRow("Dim Condition As Integer : For Condition = 0 To 9 : Next")]
    [DataRow("Dim Condition As Integer : For Condition = 9 To 0 Step -1 : Next")]
    public void IsInLoopCondition_Loops_VB(string code) =>
        IsInLoopConditionVB(code).Should().BeTrue();

    [TestMethod]
    public void IsInLoopCondition_If_VB()
    {
        const string code = """
            While A
                Do
                    If Condition Then   ' This is asserted
                    End If
                Loop While B
            End While
            """;
        IsInLoopConditionVB(code).Should().BeFalse();
    }

    [TestMethod]
    public void IsInLoopCondition_NestedInLambda()
    {
        const string code = """
            System.Action lambda = () =>
            {
                while(condition) { }
            };
            """;
        IsInLoopConditionCS(code).Should().BeTrue();
    }

    [TestMethod]
    public void MemberAccessExpression_Null_CS() =>
        CSharpSyntaxClassifier.Instance.MemberAccessExpression(CS.SyntaxFactory.IdentifierName("unexpectedNodeType")).Should().BeNull();

    [TestMethod]
    public void MemberAccessExpression_Null_VB() =>
        VisualBasicSyntaxClassifier.Instance.MemberAccessExpression(VB.SyntaxFactory.IdentifierName("unexpectedNodeType")).Should().BeNull();

    [DataTestMethod]
    [DataRow("x => condition")]
    [DataRow("x => a && (b || !condition)")]
    [DataRow("_ => condition")]
    [DataRow("(item, index) => condition")]
    public void IsInLoopCondition_LambdaInLoop_CS(string lambda) =>
        IsInLoopConditionCS($$"""while (Enumerable.Repeat(10, 10).Select({{lambda}}).Any()) { }""").Should().BeFalse();

    [DataTestMethod]
    [DataRow("Function(X) Condition")]
    [DataRow("Function(X) A AndAlso (B OrElse Not Condition)")]
    [DataRow("Function(Item, Index) Condition")]
    [DataRow("Function(X) \n Return Condition \n End Function")]
    public void IsInLoopCondition_LambdaInLoop_VB(string lambda) =>
        IsInLoopConditionVB($$"""While Enumerable.Repeat(10, 10).Select({{lambda}}).Any() : End While""").Should().BeFalse();

    private static bool IsInLoopConditionCS(string code) =>
        CSharpSyntaxClassifier.Instance.IsInLoopCondition(CreateConditionCS(code));

    private static bool IsInLoopConditionVB(string code) =>
        VisualBasicSyntaxClassifier.Instance.IsInLoopCondition(CreateConditionVB(code));

    private static SyntaxCS.IdentifierNameSyntax CreateConditionCS(string code)
    {
        var tree = TestCompiler.CompileCS($$"""
            using System.Linq;
            public class Sample
            {
                public void Method(bool a, bool b, bool condition)
                {
                    {{code}}
                }
            }
            """).Tree;
        return tree.GetRoot().DescendantNodes().OfType<SyntaxCS.IdentifierNameSyntax>().Single(x => x.NameIs("condition"));
    }

    private static SyntaxVB.IdentifierNameSyntax CreateConditionVB(string code)
    {
        var tree = TestCompiler.CompileVB($$"""
            Public Class Sample
                Private A As Boolean, B As Boolean, Condition As Boolean

                Public Sub Method()
                    {{code}}
                End Sub
            End Class
            """).Tree;
        return tree.GetRoot().DescendantNodes().OfType<SyntaxVB.IdentifierNameSyntax>().Single(x => x.NameIs("Condition"));
    }
}
