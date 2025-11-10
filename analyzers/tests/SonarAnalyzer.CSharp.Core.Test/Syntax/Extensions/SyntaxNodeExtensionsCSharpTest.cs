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

namespace SonarAnalyzer.Test.Syntax.Extensions;

[TestClass]
public class SyntaxNodeExtensionsCSharpTest
{
    [TestMethod]
    public void NameIs()
    {
        const string code = """
            public class Sample
            {
                public string Method(int arg) =>
                    arg.ToString();
            }
            """;
        var toString = CSharpSyntaxTree.ParseText(code).GetRoot().DescendantNodes().OfType<MemberAccessExpressionSyntax>().Single();
        toString.NameIs("ToString").Should().BeTrue();
        toString.NameIs("TOSTRING").Should().BeFalse();
        toString.NameIs("tostring").Should().BeFalse();
        toString.NameIs("test").Should().BeFalse();
        toString.NameIs("").Should().BeFalse();
        toString.NameIs(null).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(true, "Test")]
    [DataRow(true, "Test", "Test")]
    [DataRow(true, "Other", "Test")]
    [DataRow(false)]
    [DataRow(false, "TEST")]
    public void NameIsOrNames(bool expected, params string[] orNames)
    {
        var identifier = SyntaxFactory.IdentifierName("Test");
        identifier.NameIs("other", orNames).Should().Be(expected);
    }

    [TestMethod]
    [DataRow("Strasse", "Straße", false)] // StringComparison.InvariantCulture returns in this case and so do other cultures like de-DE
    [DataRow("\u00F6", "\u006F\u0308", false)] // 00F6 = ö; 006F = o; 0308 = https://www.fileformat.info/info/unicode/char/0308/index.htm
    [DataRow("ö", "Ö", false)]
    [DataRow("ö", "\u00F6", true)]
    public void NameIs_CultureSensitivity(string identifierName, string actual, bool expected)
    {
        var identifier = SyntaxFactory.IdentifierName(identifierName);
        identifier.NameIs(actual).Should().Be(expected);
    }

    [TestMethod]
    [DataRow(false, "Strasse", "Straße")] // StringComparison.InvariantCulture returns in this case and so do other cultures like de-DE
    [DataRow(false, "\u00F6", "\u006F\u0308")] // 00F6 = ö; 006F = o; 0308 = https://www.fileformat.info/info/unicode/char/0308/index.htm
    [DataRow(false, "ö", "\u006F\u0308", "ä", "oe")] // 006F = o; 0308 = https://www.fileformat.info/info/unicode/char/0308/index.htm
    [DataRow(false, "Köln", "Koeln", "Cologne, ", "köln")]
    [DataRow(true, "Köln", "Koeln", "Cologne, ", "K\u00F6ln")] // 00F6 = ö
    public void NameIsOrNames_CultureSensitivity(bool expected, string identifierName, string name, params string[] orNames)
    {
        var identifier = SyntaxFactory.IdentifierName(identifierName);
        identifier.NameIs(name, orNames).Should().Be(expected);
    }

    [TestMethod]
    public void NameIsOrNamesNodeWithoutName()
    {
        var returnStatement = SyntaxFactory.ReturnStatement();
        returnStatement.NameIs("A", "B", "C").Should().BeFalse();
    }

    [TestMethod]
    [DataRow("""void M() { var i = 42; }""")]
    [DataRow("""int M() => 42;""")]
    [DataRow("""Sample() : this(42) { }""")]
    [DataRow("""int field = 42;""")]
    [DataRow("""int Property { get; } = 42;""")]
    [DataRow("""int Property { get => 42; }""")]
    [DataRow("""int Property { get { return 42; } }""")]
    [DataRow("""[Priority(42)] void M() { }""")]
    [DataRow("""[Priority(Order = 42)] void M() { }""")]
    [DataRow("""void M(int i = 42) { }""")]
    public void ChangeSyntaxElement_ReturnsNewNodeAndModel(string expressionScope)
    {
        var code = $$"""
            using System;
            public class PriorityAttribute : Attribute
            {
                public PriorityAttribute() { }
                public PriorityAttribute(int priority) { }
                public int Order { get; set; }
            }

            public class Sample
            {
                public Sample(int i) { }

                {{expressionScope}}
            }
            """;
        var (tree, model) = TestCompiler.CompileCS(code);
        var literal = tree.GetRoot().DescendantNodes().OfType<LiteralExpressionSyntax>().Single();
        var newLiteral = literal.ChangeSyntaxElement(literal.WithToken(SyntaxFactory.Literal(-42)), model, out var newModel);
        newLiteral.Should().NotBeNull();
        newModel.Should().NotBeNull();
        newLiteral.Token.ValueText.Should().Be("-42");
        model.GetConstantValue(literal).Value.Should().Be(42);
        newModel.GetConstantValue(newLiteral).Value.Should().Be(-42);
    }

    [TestMethod]
    public void ChangeSyntaxElement_FailsForUnsupportedSyntaxKind()
    {
        var (tree, model) = TestCompiler.CompileCS("""namespace N { }""");
        var namespaceDeclaration = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
        var newNamespaceDeclaration = namespaceDeclaration.ChangeSyntaxElement(namespaceDeclaration.WithName(SyntaxFactory.IdentifierName("M")), model, out var newModel);
        newNamespaceDeclaration.Should().BeNull();
        newModel.Should().BeNull();
    }
}
