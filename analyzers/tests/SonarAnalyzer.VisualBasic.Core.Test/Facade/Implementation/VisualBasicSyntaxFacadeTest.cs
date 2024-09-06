/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.VisualBasic.Core.Facade.Implementation;

namespace SonarAnalyzer.VisualBasic.Core.Test.Facade.Implementation;

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
        vb.Invoking(x => x.InvocationIdentifier(SyntaxFactory.IdentifierName("ThisIsNotInvocation"))).Should().Throw<InvalidCastException>();

    [TestMethod]
    public void ModifierKinds_Null_VB() =>
        vb.ModifierKinds(null).Should().BeEmpty();

    [TestMethod]
    public void NodeExpression_Null_VB() =>
        vb.NodeExpression(null).Should().BeNull();

    [TestMethod]
    public void NodeExpression_UnexpectedTypeThrows_VB() =>
        vb.Invoking(x => x.NodeExpression(SyntaxFactory.IdentifierName("ThisTypeDoesNotHaveExpression"))).Should().Throw<InvalidOperationException>();

    [TestMethod]
    public void NodeIdentifier_Null_VB() =>
        vb.NodeIdentifier(null).Should().BeNull();

    [TestMethod]
    public void NodeIdentifier_Unexpected_Returns_Null_VB() =>
        vb.NodeIdentifier(SyntaxFactory.AttributeList()).Should().BeNull();

    [TestMethod]
    public void StringValue_UnexpectedType_VB() =>
        vb.StringValue(SyntaxFactory.ThrowStatement(), null).Should().BeNull();

    [TestMethod]
    public void StringValue_NodeIsNull_VB() =>
        vb.StringValue(null, null).Should().BeNull();

    [TestMethod]
    public void ArgumentNameColon_VB_SimpleNameWithNameColonEquals()
    {
        var expression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword));
        var argument = SyntaxFactory.SimpleArgument(SyntaxFactory.NameColonEquals(SyntaxFactory.IdentifierName("a")), expression);
        vb.ArgumentNameColon(argument).Should().BeOfType<SyntaxToken>().Subject.ValueText.Should().Be("a");
    }

    [TestMethod]
    public void ArgumentNameColon_VB_SimpleNameWithoutNameColonEquals()
    {
        var expression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword));
        var argument = SyntaxFactory.SimpleArgument(expression);
        vb.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_VB_OmittedArgument()
    {
        var argument = SyntaxFactory.OmittedArgument();
        vb.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_VB_RangeArgument()
    {
        var literal1 = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
        var literal2 = literal1.WithToken(SyntaxFactory.Literal(2));
        var argument = SyntaxFactory.RangeArgument(literal1, literal2);
        vb.ArgumentNameColon(argument).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentNameColon_VB_UnsupportedSyntaxKind()
    {
        var expression = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression, SyntaxFactory.Token(SyntaxKind.TrueKeyword));
        vb.ArgumentNameColon(expression).Should().BeNull();
    }
}
