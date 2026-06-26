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
using SonarAnalyzer.VisualBasic.Core.Trackers;

namespace SonarAnalyzer.Test.Trackers;

[TestClass]
public class ConstantValueFinderTest
{
    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, "value")]
    public void CompileTimeConstant_CS(bool strict, string expected) =>
        FindReturnedConstant_CS("""
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
    public void CompileTimeConstant_VB(bool strict, string expected) =>
        FindInspectedConstant_VB("""
            Public Class Sample
                Public Sub Method()
                    Const Local As String = "value"
                    Dim Inspected = Local
                End Sub
            End Class
            """,
            strict).Should().Be(expected);

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, "value")]
    public void LocalAssignment_CS(bool strict, string expected) =>
        FindReturnedConstant_CS("""
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
    [DataRow(true, "value")]
    public void LocalAssignment_VB(bool strict, string expected) =>
        FindInspectedConstant_VB("""
            Public Class Sample
                Public Sub Method()
                    Dim Local = "value"
                    Dim Inspected = Local
                End Sub
            End Class
            """,
            strict).Should().Be(expected);

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void FieldInitializer_OnlyResolvedInNonStrictMode_CS(bool strict, string expected) =>
        FindReturnedConstant_CS("""
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
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void FieldInitializer_OnlyResolvedInNonStrictMode_VB(bool strict, string expected) =>
        FindInspectedConstant_VB("""
            Public Class Sample
                Private Field As String = "value"

                Public Sub Method()
                    Dim Inspected = Field
                End Sub
            End Class
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
    public void ParameterDefaultValue_OnlyResolvedInNonStrictMode_CS(bool strict, string expected) =>
        FindReturnedConstant_CS("""
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
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void ParameterDefaultValue_OnlyResolvedInNonStrictMode_VB(bool strict, string expected) =>
        FindInspectedConstant_VB("""
            Public Class Sample
                Public Sub Method(Optional Parameter As String = "value")
                    Dim Inspected = Parameter
                End Sub
            End Class
            """,
            strict).Should().Be(expected);

    [TestMethod]
    public void ParameterDefaultValue_NotResolvedWhenConditionallyReassigned_CS() =>
        FindReturnedConstant_CS("""
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
    public void ParameterDefaultValue_NotResolvedWhenConditionallyReassigned_VB() =>
        FindInspectedConstant_VB("""
            Public Class Sample
                Public Sub Method(condition As Boolean, Optional parameter As String = "value")
                    If condition Then
                        Parameter = "other"
                    End If
                    Dim Inspected = Parameter
                End Sub
            End Class
            """,
            false).Should().BeNull();

    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void ParameterDefaultValue_PartialMethod_ResolvedFromAuthoritativeDeclaration_CS(bool strict, string expected) =>
        FindReturnedConstant_CS("""
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
    public void ParameterDefaultValue_PartialMethod_ResolvedFromAuthoritativeDeclaration_VB(bool strict, string expected) =>
        FindInspectedConstant_VB("""
            Partial Public Class Sample
                Partial Private Sub Method(Optional parameter As String = "value")
                End Sub

                Private Sub Method(Optional parameter As String = "value")
                    Dim Inspected = Parameter
                End Sub
            End Class
            """,
            strict).Should().Be(expected);

    // This test has no VB counterpart because VB does not allow partial methods to have conflicting default values.
    [TestMethod]
    [DataRow(false, "value")]
    [DataRow(true, null)]
    public void ParameterDefaultValue_PartialMethod_ConflictingDefaults_ResolvedFromDefiningDeclaration_CS(bool strict, string expected) =>
        // The default on the defining declaration is authoritative; the implementing declaration's is ignored (CS1066).
        FindReturnedConstant_CS("""
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

    // This test has no VB counterpart because VB does not allow partial methods to have conflicting default values.
    [TestMethod]
    public void ParameterDefaultValue_PartialMethod_DefaultOnlyOnImplementation_NotResolved_CS() =>
        // The defining declaration has no default, so the parameter is required and the implementation's default has no effect (CS1066). There is no usable default value.
        FindReturnedConstant_CS("""
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

    private static object FindReturnedConstant_CS(string code, bool strict)
    {
        var (tree, model) = TestCompiler.CompileCS(code);
        var returnExpression = tree.Single<Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax>().Expression;
        return new CSharpConstantValueFinder(model, strict).FindConstant(returnExpression);
    }

    // VB partial methods must be 'Sub' (BC31437) and therefore cannot return a value, so - unlike the C# helper - we
    // cannot anchor on a return statement. Instead the expression under test is assigned to a local named 'inspected'.
    private static object FindInspectedConstant_VB(string code, bool strict)
    {
        var (tree, model) = TestCompiler.CompileVB(code);
        var inspectedValue = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.VariableDeclaratorSyntax>()
            .Single(x => x.Names.Any(name => name.Identifier.ValueText == "Inspected"))
            .Initializer
            .Value;
        return new VisualBasicConstantValueFinder(model, strict).FindConstant(inspectedValue);
    }
}
