/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.Test.Trackers;

[TestClass]
public class ConstantValueFinderTest
{
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
        var finder = new CSharpConstantValueFinder(compilation.GetSemanticModel(tree));
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
        var firstCompilationFinder = new CSharpConstantValueFinder(firstCompilation.GetSemanticModel(firstCompilation.SyntaxTrees.Single()));
        firstCompilationFinder.FindConstant(secondCompilationReturnExpression).Should().BeNull();
        var secondCompilationFinder = new CSharpConstantValueFinder(secondCompilation.GetSemanticModel(secondCompilation.SyntaxTrees.Single()));
        secondCompilationFinder.FindConstant(secondCompilationReturnExpression).Should().NotBeNull();
    }
}
