/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    [TestMethod]
    [DataRow("null", false)]
    [DataRow("var o = new object();", true)]
    [DataRow("int? x = 1", true)]
    [DataRow("int x = 1;", false)]
    public void CanBeNull(string code, bool expected)
    {
        var (expression, semanticModel) = Compile(code);

        expression.CanBeNull(semanticModel).Should().Be(expected);
    }

    [TestMethod]
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

    [TestMethod]
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
        var result = parsed.LeftMostInMemberAccess;
        var asString = result?.ToString() ?? "null";
        asString.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    [DataRow("$$a$$;", "a")]
    [DataRow("$$a$$.b;", "a.b")]
    [DataRow("$$a$$.b();", "a.b()")]
    [DataRow("$$a$$.b().c;", "a.b().c")]
    [DataRow("$$a$$();", "a()")]
    [DataRow("$$a$$[b];", "a[b]")]
    [DataRow("$$a$$!;", "a!")]
    [DataRow("$$a$$!.b();", "a!.b()")]
    [DataRow("$$a$$?.b();", "a?.b()")]
    [DataRow("a?.$$b!$$.c();", "a?.b!.c()")] // Climbing from the WhenNotNull side of "?." must still reach the outer root
    [DataRow("($$a$$.b()).c();", "(a.b()).c()")] // Intermediate parentheses must not stop the climb
    [DataRow("await $$a$$.b();", "a.b()")] // The climb stops below "await", it is not part of the receiver chain
    public void ChainRoot(string expression, string expected) =>
        TestCompiler.NodeBetweenMarkersCS<ExpressionSyntax>(expression, ignoreErrors: true).Node.ChainRoot.ToString().Should().Be(expected);

    [TestMethod]
    [DataRow("default", true)]
    [DataRow("default!", true)]
    [DataRow("(default)!", true)]
    [DataRow("(default!)", true)]
    [DataRow("((default)!)", true)]
    [DataRow("default(int)", false)]
    [DataRow("default(int)!", false)]
    [DataRow("(default(int)!)", false)]
    [DataRow("(1 + 1)", false)]
    [DataRow("", false)]
    [DataRow("()", false)]
    public void IsDefaultLiteral(string expression, bool expected)
    {
        var parsed = SyntaxFactory.ParseExpression(expression);
        var result = parsed.IsDefaultLiteral;
        result.Should().Be(expected);
    }

    [TestMethod]
    public void IsDefaultLiteral_Null() =>
        ((ExpressionSyntax)null).IsDefaultLiteral.Should().BeFalse();

    [TestMethod]
    [DataRow("$$a.b.c$$();", "a.b")]
    [DataRow("$$a.b$$();", "a")]
    [DataRow("$$a.b().c$$();", "a.b()")]
    [DataRow("$$a$$();", null)]
    [DataRow("a?$$.b$$();", "a")]
    [DataRow("a?.b?$$.c$$();", ".b")]
    [DataRow("$$a!.b$$();", "a")]
    [DataRow("$$a!!.b$$();", "a")]
    [DataRow("$$a.b()!.c$$();", "a.b()")]
    [DataRow("a?$$.b!.c$$();", ".b")]
    [DataRow("a!?$$.b$$();", "a")]
    [DataRow("$$a!$$();", null)]
    [DataRow("$$a.b!$$();", "a")]
    public void LeftOfDot(string expression, string expected)
    {
        var node = TestCompiler.NodeBetweenMarkersCS<ExpressionSyntax>(expression, ignoreErrors: true).Node;
        (node.LeftOfDot?.ToString()).Should().Be(expected);
    }

    private static (ExpressionSyntax Expression, SemanticModel Model) Compile(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TempAssembly.dll").AddSyntaxTrees(tree).AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);
        var model = compilation.GetSemanticModel(tree);
        return (tree.First<ExpressionSyntax>(), model);
    }
}
