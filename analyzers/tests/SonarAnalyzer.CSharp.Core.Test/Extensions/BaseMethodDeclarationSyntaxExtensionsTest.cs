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

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SonarAnalyzer.CSharp.Core.Test.Extensions;

[TestClass]
public class BaseMethodDeclarationSyntaxExtensionsTest
{
    [TestMethod]
    public void GivenNullMethodDeclaration_GetBodyDescendantNodes_ThrowsArgumentNullException()
    {
#if NETFRAMEWORK
        var messageFormat = "Value cannot be null." + Environment.NewLine + "Parameter name: {0}";
#else
        var messageFormat = "Value cannot be null. (Parameter '{0}')";
#endif

        BaseMethodDeclarationSyntax sut = null;

        var exception = Assert.ThrowsException<ArgumentNullException>(() => sut.GetBodyDescendantNodes());

        exception.Message.Should().Be(string.Format(messageFormat, "method"));
    }

    [TestMethod]
    [DynamicData(nameof(GetMethodDeclarationsAndExpectedBody), DynamicDataSourceType.Method)]
    public void HasBodyOrExpressionBody(BaseMethodDeclarationSyntax methodDeclaration, SyntaxNode expectedBody)
    {
        var hasBody = methodDeclaration.HasBodyOrExpressionBody();
        if (expectedBody is null)
        {
            hasBody.Should().BeFalse();
        }
        else
        {
            hasBody.Should().BeTrue();
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetMethodDeclarationsAndExpectedBody), DynamicDataSourceType.Method)]
    public void GetBodyOrExpressionBody(BaseMethodDeclarationSyntax methodDeclaration, SyntaxNode expectedBody) =>
        methodDeclaration.GetBodyOrExpressionBody().Should().Be(expectedBody);

    private static IEnumerable<object[]> GetMethodDeclarationsAndExpectedBody()
    {
        var methodWithBody = Method().WithBody(Block());
        var expressionBody = ArrowExpressionClause(LiteralExpression(SyntaxKind.TrueLiteralExpression));
        var methodWithExpressionBody = Method().WithExpressionBody(expressionBody);
        var methodWithBoth = Method().WithBody(Block()).WithExpressionBody(expressionBody);

        // Corresponds to (BaseMethodDeclarationSyntax methodDeclaration, SyntaxNode expectedBody)
        yield return new object[] { null, null };
        yield return new object[] { Method(), null };
        yield return new object[] { methodWithBody, methodWithBody.Body };
        yield return new object[] { methodWithExpressionBody, methodWithExpressionBody.ExpressionBody.Expression };
        yield return new object[] { methodWithBoth, methodWithBoth.Body };

        static BaseMethodDeclarationSyntax Method() =>
            MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "Test");
    }
}
