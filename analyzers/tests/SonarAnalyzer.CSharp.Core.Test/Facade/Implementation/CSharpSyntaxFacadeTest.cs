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

namespace SonarAnalyzer.CSharp.Core.Facade.Implementation.Test;

[TestClass]
public class CSharpSyntaxFacadeTest
{
    private readonly CSharpSyntaxFacade cs = new();

    [TestMethod]
    public void EnumMembers_Null_CS() =>
        cs.EnumMembers(null).Should().BeEmpty();

    [TestMethod]
    public void InvocationIdentifier_Null_CS() =>
        cs.InvocationIdentifier(null).Should().BeNull();

    [TestMethod]
    public void ObjectCreationTypeIdentifier_Null_CS() =>
        cs.ObjectCreationTypeIdentifier(null).Should().BeNull();

    [TestMethod]
    public void InvocationIdentifier_UnexpectedTypeThrows_CS() =>
        cs.Invoking(x => x.InvocationIdentifier(SyntaxFactory.IdentifierName("ThisIsNotInvocation"))).Should().Throw<InvalidCastException>();

    [TestMethod]
    public void ModifierKinds_Null_CS() =>
        cs.ModifierKinds(null).Should().BeEmpty();

    [TestMethod]
    public void NodeExpression_Null_CS() =>
        cs.NodeExpression(null).Should().BeNull();

    [TestMethod]
    public void NodeExpression_UnexpectedTypeThrows_CS() =>
        cs.Invoking(x => x.NodeExpression(SyntaxFactory.IdentifierName("ThisTypeDoesNotHaveExpression"))).Should().Throw<InvalidOperationException>();

    [TestMethod]
    public void NodeIdentifier_Null_CS() =>
        cs.NodeIdentifier(null).Should().BeNull();

    [TestMethod]
    public void NodeIdentifier_Unexpected_Returns_Null_CS() =>
       cs.NodeIdentifier(SyntaxFactory.AttributeList()).Should().BeNull();

    [TestMethod]
    public void StringValue_UnexpectedType_CS() =>
         cs.StringValue(SyntaxFactory.ThrowStatement(), null).Should().BeNull();

    [TestMethod]
    public void StringValue_NodeIsNull_CS() =>
        cs.StringValue(null, null).Should().BeNull();

    [TestMethod]
    public void RemoveConditionalAccess_Null_CS() =>
        cs.RemoveConditionalAccess(null).Should().BeNull();

    [TestMethod]
    [DataRow("M()", "M()")]
    [DataRow("this.M()", "this.M()")]
    [DataRow("A.B.C.M()", "A.B.C.M()")]
    [DataRow("A.B?.C.M()", ".C.M()")]
    [DataRow("A.B?.C?.M()", ".M()")]
    [DataRow("A.B?.C?.D", ".D")]
    public void RemoveConditionalAccess_SimpleInvocation_CS(string invocation, string expected) =>
        cs.RemoveConditionalAccess(SyntaxFactory.ParseExpression(invocation)).ToString().Should().Be(expected);

    [TestMethod]
    public void ArgumentNameColon_CS_WithNameColon()
    {
        var expression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        var argument = SyntaxFactory.Argument(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("a")), SyntaxFactory.Token(SyntaxKind.None), expression);
        cs.ArgumentNameColon(argument).Should().BeOfType<SyntaxToken>().Subject.ValueText.Should().Be("a");
    }

    [TestMethod]
    public void ArgumentNameColon_CS_WithoutNameColon()
    {
        var expression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        var argument = SyntaxFactory.Argument(expression);
        cs.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_CS_UnsupportedSyntaxKind()
    {
        var expression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword));
        cs.ArgumentNameColon(expression).Should().BeNull();
    }
}
