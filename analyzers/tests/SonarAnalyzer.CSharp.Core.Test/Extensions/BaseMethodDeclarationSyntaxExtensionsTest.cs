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

        var exception = Assert.Throws<ArgumentNullException>(sut.GetBodyDescendantNodes);

        exception.Message.Should().Be(string.Format(messageFormat, "method"));
    }

    [TestMethod]
    [DynamicData(nameof(GetMethodDeclarationsAndExpectedBody))]
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
