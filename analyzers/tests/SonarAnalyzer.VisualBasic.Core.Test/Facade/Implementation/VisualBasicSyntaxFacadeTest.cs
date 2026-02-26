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

using SonarAnalyzer.Core.Syntax.Utilities;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace SonarAnalyzer.VisualBasic.Core.Facade.Implementation.Test;

[TestClass]
public class VisualBasicSyntaxFacadeTest
{
    private readonly VisualBasicSyntaxFacade vb = new();

    [TestMethod]
    public void EnumMembers_Null_VB() =>
        vb.EnumMembers(null).Should().BeEmpty();

    [TestMethod]
    public void InvocationIdentifier_Null_VB() =>
        vb.InvocationIdentifier(null).Should().BeNull();

    [TestMethod]
    public void ObjectCreationTypeIdentifier_Null_VB() =>
        vb.ObjectCreationTypeIdentifier(null).Should().BeNull();

    [TestMethod]
    public void InvocationIdentifier_UnexpectedTypeThrows_VB() =>
        vb.Invoking(x => x.InvocationIdentifier(IdentifierName("ThisIsNotInvocation"))).Should().Throw<InvalidCastException>();

    [TestMethod]
    public void ModifierKinds_Null_VB() =>
        vb.ModifierKinds(null).Should().BeEmpty();

    [TestMethod]
    public void NodeExpression_Null_VB() =>
        vb.NodeExpression(null).Should().BeNull();

    [TestMethod]
    public void NodeExpression_UnexpectedTypeThrows_VB() =>
        vb.Invoking(x => x.NodeExpression(IdentifierName("ThisTypeDoesNotHaveExpression"))).Should().Throw<InvalidOperationException>();

    [TestMethod]
    public void NodeIdentifier_Null_VB() =>
        vb.NodeIdentifier(null).Should().BeNull();

    [TestMethod]
    public void NodeIdentifier_Unexpected_Returns_Null_VB() =>
        vb.NodeIdentifier(AttributeList()).Should().BeNull();

    [TestMethod]
    public void StringValue_UnexpectedType_VB() =>
        vb.StringValue(ThrowStatement(), null).Should().BeNull();

    [TestMethod]
    public void StringValue_NodeIsNull_VB() =>
        vb.StringValue(null, null).Should().BeNull();

    [TestMethod]
    public void ArgumentNameColon_VB_SimpleNameWithNameColonEquals()
    {
        var expression = LiteralExpression(SyntaxKind.TrueLiteralExpression, Token(SyntaxKind.TrueKeyword));
        var argument = SimpleArgument(NameColonEquals(IdentifierName("a")), expression);
        vb.ArgumentNameColon(argument).Should().BeOfType<SyntaxToken>().Subject.ValueText.Should().Be("a");
    }

    [TestMethod]
    public void ArgumentNameColon_VB_SimpleNameWithoutNameColonEquals()
    {
        var expression = LiteralExpression(SyntaxKind.TrueLiteralExpression, Token(SyntaxKind.TrueKeyword));
        var argument = SimpleArgument(expression);
        vb.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_VB_OmittedArgument()
    {
        var argument = OmittedArgument();
        vb.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_VB_RangeArgument()
    {
        var literal1 = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1));
        var literal2 = literal1.WithToken(Literal(2));
        var argument = RangeArgument(literal1, literal2);
        vb.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_VB_UnsupportedSyntaxKind()
    {
        var expression = LiteralExpression(SyntaxKind.TrueLiteralExpression, Token(SyntaxKind.TrueKeyword));
        vb.ArgumentNameColon(expression).Should().BeNull();
    }

    [TestMethod]
    public void ComparisonKind_BinaryExpression_VB()
    {
        var binary = BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName("a"), Token(SyntaxKind.EqualsToken), IdentifierName("b"));
        vb.ComparisonKind(binary).Should().Be(ComparisonKind.Equals);
    }

    [TestMethod]
    public void ComparisonKind_NonBinaryExpression_VB() =>
        vb.ComparisonKind(IdentifierName("a")).Should().Be(ComparisonKind.None);

    [TestMethod]
    public void ComparisonKind_Null_VB() =>
        vb.ComparisonKind(null).Should().Be(ComparisonKind.None);
}
