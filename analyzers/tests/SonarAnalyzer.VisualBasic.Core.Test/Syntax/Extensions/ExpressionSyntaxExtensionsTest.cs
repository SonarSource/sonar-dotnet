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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions.Test;

[TestClass]
public class ExpressionSyntaxExtensionsTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("a", "a")]
    [DataRow("Nothing", "null")]
    [DataRow("a + b", "null")]
    [DataRow("Me.a", "a")]
    [DataRow("Me.a.b", "a")]
    [DataRow("a.b", "a")]
    [DataRow("a.b()", "a")]
    [DataRow("a.b().c", "a")]
    [DataRow("a()", "a")]
    [DataRow("a().b", "a")]
    [DataRow("(a.b).c", "a")]
    [DataRow("a.b?.c.d(e)?(f).g?.h", "a")]
    [DataRow("Integer.MaxValue", "Integer")]
    [DataRow("a?.b", "a")]
    [DataRow("a?.b?.c", "a")]
    public void LeftMostInMemberAccess(string expression, string expected) =>
        (SyntaxFactory.ParseExpression(expression).LeftMostInMemberAccess?.ToString() ?? "null").Should().BeEquivalentTo(expected);

    [TestMethod]
    [DataRow("a", "a")]
    [DataRow("(a)", "a")]
    [DataRow("((a))", "a")]
    [DataRow("(((a)))", "a")]
    [DataRow("(a + b)", "a + b")]
    [DataRow("((a + b))", "a + b")]
    public void RemoveParentheses(string expression, string expected) =>
        SyntaxFactory.ParseExpression(expression).RemoveParentheses().ToString().Should().Be(expected);

    [TestMethod]
    [DataRow("a = b", "a", true)]
    [DataRow("a = b", "b", false)]
    [DataRow("a.b = c", "a.b", true)]
    [DataRow("a.b = c", "c", false)]
    public void IsLeftSideOfAssignment(string statement, string expressionToFind, bool expected) =>
        Parse(statement).OfType<ExpressionSyntax>().First(x => x.ToString() == expressionToFind).IsLeftSideOfAssignment.Should().Be(expected);

    [TestMethod]
    [DataRow("MyBase.Method()", true)]
    [DataRow("MyBase.Property", true)]
    [DataRow("Me.Method()", false)]
    [DataRow("Me.Property", false)]
    [DataRow("Other.Method()", false)]
    [DataRow("Method()", true)]
    [DataRow("SomeIdentifier", true)]
    [DataRow("MyBase?.Method()", true)]
    [DataRow("Me?.Method()", false)]
    [DataRow("Other?.Method()", false)]
    [DataRow("42", false)]
    public void IsOnBase(string expression, bool expected) =>
        Parse($"Dim x = {expression}").OfType<ExpressionSyntax>().First(x => x.ToString() == expression).IsOnBase.Should().Be(expected);

    [TestMethod]
    [DataRow("a.b", "a.b")]
    [DataRow("(a.b)", "a.b")]
    [DataRow("((a.b))", "a.b")]
    public void SelfOrTopParenthesizedExpression(string outerExpression, string innerExpression) =>
        Parse($"Dim x = {outerExpression}").OfType<MemberAccessExpressionSyntax>().First(x => x.ToString() == innerExpression)
            .SelfOrTopParenthesizedExpression
            .ToString()
            .Should().Be(outerExpression);

    [TestMethod]
    [DataRow("ToString", true)]
    [DataRow("TOSTRING", true)]
    [DataRow("tostring", true)]
    [DataRow("Other", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void NameIs(string name, bool expected) =>
        Parse("Dim x = obj.ToString()").OfType<MemberAccessExpressionSyntax>().Single().NameIs(name).Should().Be(expected);

    [TestMethod]
    [DataRow("42", true)]
    [DataRow("\"Hello\"", true)]
    [DataRow("True", true)]
    [DataRow("False", true)]
    [DataRow("Nothing", true)]
    [DataRow("\"c\"c", true)]
    public void HasConstantValue_Literals(string expression, bool expected)
    {
        var (tree, model) = Compile($"Dim x = {expression}");
        tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().Initializer.Value.HasConstantValue(model).Should().Be(expected);
    }

    [TestMethod]
    public void HasConstantValue_NonConstant_Parameter()
    {
        var (tree, model) = Compile("Dim x = a", "Sub M(a As Integer)");
        tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().Initializer.Value.HasConstantValue(model).Should().BeFalse();
    }

    [TestMethod]
    public void HasConstantValue_NonConstant_MethodCall()
    {
        var (tree, model) = Compile("Dim x = GetValue()", classMembers: """
            Function GetValue() As Integer
                Return 42
            End Function
            """);
        tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<VariableDeclaratorSyntax>().Single().Initializer.Value.HasConstantValue(model).Should().BeFalse();
    }

    [TestMethod]
    public void HasConstantValue_ConstantField()
    {
        var (tree, model) = Compile("Dim x = MyConst", classMembers: "Const MyConst As Integer = 42");
        tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<VariableDeclaratorSyntax>().Last().Initializer.Value.HasConstantValue(model).Should().BeTrue();
    }

    [TestMethod]
    [DataRow("a?.b", "a?.b")]
    [DataRow("a?.b?.c", "a?.b?.c")]
    [DataRow("a?.b.c", "a?.b.c")]
    public void RootConditionalAccessExpression(string fullExpression, string expectedRoot) =>
        SyntaxFactory.ParseExpression(fullExpression).DescendantNodesAndSelf().OfType<ConditionalAccessExpressionSyntax>().First().RootConditionalAccessExpression.ToString().Should().Be(expectedRoot);

    private static SyntaxNode[] Parse(string methodBody, string methodSignature = "Sub M()", string classMembers = null) =>
        VisualBasicSyntaxTree.ParseText(WrapInClass(methodBody, methodSignature, classMembers)).GetRoot().DescendantNodes().ToArray();

    private static (SyntaxTree Tree, SemanticModel Model) Compile(string methodBody, string methodSignature = "Sub M()", string classMembers = null) =>
        TestCompiler.CompileVB(WrapInClass(methodBody, methodSignature, classMembers));

    private static string WrapInClass(string methodBody, string methodSignature = "Sub M()", string classMembers = null) =>
        $"""
        Class C
            {classMembers}
            {methodSignature}
                {methodBody}
            End Sub
        End Class
        """;
}
