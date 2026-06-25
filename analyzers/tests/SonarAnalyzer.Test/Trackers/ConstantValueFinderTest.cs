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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.Test.Trackers;

[TestClass]
public class ConstantValueFinderTest
{
    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, "value")]
    public void CompileTimeConstant(bool strict, string expected) =>
        FindReturnedConstant("""
            public class Sample
            {
                public object Method()
                {
                    const string local = "value";
                    return local;
                }
            }
            """,
            strict).Should().Be(expected);

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, "value")]
    public void LocalAssignment(bool strict, string expected) =>
        FindReturnedConstant("""
            public class Sample
            {
                public object Method()
                {
                    var local = "value";
                    return local;
                }
            }
            """,
            strict).Should().Be(expected);

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void FieldInitializer_OnlyResolvedInNonStrictMode(bool strict, string expected) =>
        FindReturnedConstant("""
            public class Sample
            {
                private string field = "value";
                public object Method()
                {
                    return field;
                }
            }
            """,
            strict).Should().Be(expected);

    [TestMethod]
    public void FieldDeclaredInAnotherSyntaxTree()
    {
        const string code1 = @"
public partial class Sample
{
    private static int Original = 42;
    private int Field = Original;
}";
        const string code2 = @"
public partial class Sample
{
    public int Method()
    {
        return Field;
    }
}";
        var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippets(code1, code2).GetCompilation();
        var tree = compilation.SyntaxTrees.Single(x => x.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
        var returnExpression = tree.Single<ReturnStatementSyntax>().Expression;
        var finder = new CSharpConstantValueFinder(compilation.GetSemanticModel(tree), false);
        finder.FindConstant(returnExpression).Should().Be(42);
    }

    [TestMethod]
    public void WrongCompilationBeingUsed()
    {
        const string firstSnippet = @"
public class Foo
{
    private static int Original = 42;
    private int Field = Original;
}";
        const string secondSnippet = @"
public class Bar
{
    private int Field = 42;
    public int Method()
    {
        return Field;
    }
}";
        var firstCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(firstSnippet).GetCompilation();
        var secondCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(secondSnippet).GetCompilation();
        var secondCompilationReturnExpression = secondCompilation.SyntaxTrees.Single().Single<ReturnStatementSyntax>().Expression;
        var firstCompilationFinder = new CSharpConstantValueFinder(firstCompilation.GetSemanticModel(firstCompilation.SyntaxTrees.Single()), false);
        firstCompilationFinder.FindConstant(secondCompilationReturnExpression).Should().BeNull();
        var secondCompilationFinder = new CSharpConstantValueFinder(secondCompilation.GetSemanticModel(secondCompilation.SyntaxTrees.Single()), false);
        secondCompilationFinder.FindConstant(secondCompilationReturnExpression).Should().NotBeNull();
    }

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void ParameterDefaultValue_OnlyResolvedInNonStrictMode(bool strict, string expected) =>
        FindReturnedConstant("""
            public class Sample
            {
                public object Method(string parameter = "value")
                {
                    return parameter;
                }
            }
            """,
            strict).Should().Be(expected);

    [TestMethod]
    public void ParameterDefaultValue_NotResolvedWhenConditionallyReassigned() =>
        FindReturnedConstant("""
            public class Sample
            {
                public object Method(bool condition, string parameter = "value")
                {
                    if (condition)
                    {
                        parameter = "other";
                    }
                    return parameter;
                }
            }
            """,
            false).Should().BeNull();

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void ParameterDefaultValue_PartialMethod_ResolvedFromAuthoritativeDeclaration(bool strict, string expected) =>
        FindReturnedConstant("""
            public partial class Sample
            {
                public partial object Method(string parameter = "value");
                public partial object Method(string parameter)
                {
                    return parameter;
                }
            }
            """,
            strict).Should().Be(expected);

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void ParameterDefaultValue_PartialMethod_ConflictingDefaults_ResolvedFromDefiningDeclaration(bool strict, string expected) =>
        // The default on the defining declaration is authoritative; the implementing declaration's is ignored (CS1066).
        FindReturnedConstant("""
            public partial class Sample
            {
                public partial object Method(string parameter = "value");
                public partial object Method(string parameter = "other")
                {
                    return parameter;
                }
            }
            """,
            strict).Should().Be(expected);

    [TestMethod]
    public void ParameterDefaultValue_PartialMethod_DefaultOnlyOnImplementation_NotResolved() =>
        // The defining declaration has no default, so the parameter is required and the implementation's default has no effect (CS1066). There is no usable default value.
        FindReturnedConstant("""
            public partial class Sample
            {
                public partial object Method(string parameter);
                public partial object Method(string parameter = "other")
                {
                    return parameter;
                }
            }
            """,
            false).Should().BeNull();

    private static object FindReturnedConstant(string code, bool strict)
    {
        var (tree, model) = TestCompiler.CompileCS(code);
        var returnExpression = tree.Single<ReturnStatementSyntax>().Expression;
        return new CSharpConstantValueFinder(model, strict).FindConstant(returnExpression);
    }
}
