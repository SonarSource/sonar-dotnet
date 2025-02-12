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

namespace SonarAnalyzer.Test.Syntax.Extensions;

[TestClass]
public partial class SyntaxNodeExtensionsVisualBasicTest
{
    private const string Source = """
        Class Example
            Function Method(Input As Object) As String
                Dim Qualified As System.Exception
                Dim GlobalName As Global.System.Exception
                Return Input.ToString()
            End Function
        End Class
        """;

    [TestMethod]
    public void GetName()
    {
        var nodes = Parse(Source);
        GetName(nodes.OfType<ClassStatementSyntax>().Single()).Should().Be("Example");
        GetName(nodes.OfType<MethodBlockSyntax>().Single()).Should().Be("Method");
        GetName(nodes.OfType<MethodStatementSyntax>().Single()).Should().Be("Method");
        GetName(nodes.OfType<ParameterSyntax>().Single()).Should().Be("Input");
        GetName(nodes.OfType<PredefinedTypeSyntax>().First()).Should().Be("Object");
        GetName(nodes.OfType<MemberAccessExpressionSyntax>().Single()).Should().Be("ToString");
        nodes.OfType<IdentifierNameSyntax>().Select(x => GetName(x)).Should().BeEquivalentTo("System", "Exception", "System", "Exception", "Input", "ToString");
        nodes.OfType<QualifiedNameSyntax>().Select(x => GetName(x)).Should().BeEquivalentTo("Exception", "Exception", "System");
        GetName(nodes.OfType<InvocationExpressionSyntax>().Single()).Should().Be("ToString");
        GetName(nodes.OfType<ReturnStatementSyntax>().Single()).Should().BeEmpty();

        static string GetName(SyntaxNode node) => SyntaxNodeExtensionsVisualBasic.GetName(node);
    }

    [TestMethod]
    public void IsNothingLiteral_Null() =>
        SyntaxNodeExtensionsVisualBasic.IsNothingLiteral(null).Should().BeFalse();

    [TestMethod]
    public void NameIs()
    {
        var toString = Parse(Source).OfType<MemberAccessExpressionSyntax>().Single();
        toString.NameIs("ToString").Should().BeTrue();
        toString.NameIs("TOSTRING").Should().BeTrue();
        toString.NameIs("tostring").Should().BeTrue();
        toString.NameIs("test").Should().BeFalse();
        toString.NameIs("").Should().BeFalse();
        toString.NameIs(null).Should().BeFalse();
    }

    private static SyntaxNode[] Parse(string source) =>
        VisualBasicSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();
}
