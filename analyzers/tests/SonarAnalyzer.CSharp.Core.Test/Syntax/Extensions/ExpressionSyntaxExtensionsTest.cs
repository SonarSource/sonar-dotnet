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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class ExpressionSyntaxExtensionsTest
{
    [DataTestMethod]
    [DataRow("null", false)]
    [DataRow("var o = new object();", true)]
    [DataRow("int? x = 1", true)]
    [DataRow("int x = 1;", false)]
    public void CanBeNull(string code, bool expected)
    {
        var (expression, semanticModel) = Compile(code);

        expression.CanBeNull(semanticModel).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("a", "a")]
    [DataRow("a + b", "a", "b")]
    [DataRow("a++", "a")]
    [DataRow("++a", "a")]
    [DataRow("a.b", "a.b")]
    [DataRow("a.b()", "a.b()")]
    [DataRow("a.b() + 1", "a.b()")]
    [DataRow("a.b() + b().c", "a.b()", "b().c")]
    [DataRow("a!.b()", "a!.b()")]
    [DataRow("a?.b()", "a?.b()")]
    [DataRow("a.b()?.c.d?[e].f?.g", "a.b()?.c.d?[e].f?.g")] // Should also return "e"
    [DataRow("a(b, c)", "a(b, c)", "b", "c")]
    [DataRow("a[b, c]]", "a[b, c]", "b", "c")]
    [DataRow("(a)", "a")]
    [DataRow("a as b", "a")]
    [DataRow("a is b", "a")]
    [DataRow("a is b c", "a")]
    [DataRow("(a)b", "b")]
    [DataRow("await a", "a")]
    [DataRow("a!", "a")]
    [DataRow("""  $"{a} {b}" """, "a", "b")]
    [DataRow("""" $"""{a} {b}""" """", "a", "b")]
    [DataRow("a switch { b c => d }", "a", "d")]
    [DataRow("a switch { b => c, { d: { } } => e }", "a", "c", "e")]
    public void ExtractMemberIdentifier(string expression, params string[] memberIdentifiers)
    {
        var parsed = SyntaxFactory.ParseExpression(expression);
        var result = parsed.ExtractMemberIdentifier();
        var asString = result.Select(x => x.ToString());
        asString.Should().BeEquivalentTo(memberIdentifiers);
    }

    [DataTestMethod]
    [DataRow("a", "a")]
    [DataRow("null", "null")]
    [DataRow("a + b", "null")]
    [DataRow("this.a", "a")]
    [DataRow("this.a.b", "a")]
    [DataRow("a.b", "a")]
    [DataRow("a.b()", "a")]
    [DataRow("a.b().c", "a")]
    [DataRow("a()", "a")]
    [DataRow("a().b", "a")]
    [DataRow("a()!.b", "a")]
    [DataRow("(a.b).c", "a")]
    [DataRow("a.b?.c.d[e]?[f].g?.h", "a")]
    [DataRow("a[b]", "a")]
    [DataRow("a?[b]", "a")]
    [DataRow("a->b", "a")]
    [DataRow("int.MaxValue", "int")]
    public void GetLeftMostInMemberAccess(string expression, string expected)
    {
        var parsed = SyntaxFactory.ParseExpression(expression);
        var result = parsed.GetLeftMostInMemberAccess();
        var asString = result?.ToString() ?? "null";
        asString.Should().BeEquivalentTo(expected);
    }

    private static (ExpressionSyntax Expression, SemanticModel Model) Compile(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TempAssembly.dll").AddSyntaxTrees(tree).AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);
        var model = compilation.GetSemanticModel(tree);
        return (tree.First<ExpressionSyntax>(), model);
    }
}
