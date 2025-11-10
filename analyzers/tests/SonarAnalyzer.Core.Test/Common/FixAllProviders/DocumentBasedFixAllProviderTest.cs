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

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Core.Test.Common.FixAllProviders;

[TestClass]
public class DocumentBasedFixAllProviderTest
{
    [TestMethod]
    [DataRow(FixAllScope.Document, "Fix all 'SDummy' in 'MyFile1.cs'")]
    [DataRow(FixAllScope.Project, "Fix all 'SDummy' in 'project0'")]
    [DataRow(FixAllScope.Solution, "Fix all 'SDummy' in Solution")]
    public async Task GetFixAsync_ForSupportedScope_HasCorrectTitle(FixAllScope scope, string expectedTitle)
    {
        var codeFix = new DummyCodeFixCS();
        var document = CreateProject().FindDocument("MyFile1.cs");
        var fixAllContext = new FixAllContext(document, codeFix, scope, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(null), default);
        var result = await SonarAnalyzer.Core.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext);
        result.Title.Should().Be(expectedTitle);
    }

    [TestMethod]
    public async Task GetFixAsync_ForUnsupportedScope_ReturnsNull()
    {
        var codeFix = new DummyCodeFixCS();
        var document = CreateProject().FindDocument("MyFile1.cs");
        var fixAllContext = new FixAllContext(document, codeFix, FixAllScope.Custom, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(null), default);
        var result = await SonarAnalyzer.Core.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext);
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetFixAsync_ForDocument_OnlyTheDocumentChanged()
    {
        var codeFix = new DummyCodeFixCS();
        var project = CreateProject();
        var document1Before = project.FindDocument("MyFile1.cs");
        var document2Before = project.FindDocument("MyFile2.cs");
        var compilation = project.GetCompilation();
        var diagnostics = DiagnosticVerifier.AnalyzerDiagnostics(compilation, new DummyAnalyzerCS(), CompilationErrorBehavior.Ignore).ToImmutableArray();

        var fixAllContext = new FixAllContext(document1Before, codeFix, FixAllScope.Document, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(diagnostics), default);
        var result = await SonarAnalyzer.Core.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext);
        var executedOperation = await result.GetOperationsAsync(default);
        var document1After = executedOperation.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document1Before.Id);
        var document2After = executedOperation.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document2Before.Id);

        var document1BeforeRoot = await document1Before.GetSyntaxRootAsync();
        var document1BeforeContent = document1BeforeRoot.GetText().ToString();
        var document1AfterRoot = await document1After.GetSyntaxRootAsync();
        var document1AfterContent = document1AfterRoot.GetText().ToString();

        document1BeforeContent.Should().NotBe(document1AfterContent);

        var document2BeforeRoot = await document2Before.GetSyntaxRootAsync();
        var document2BeforeContent = document2BeforeRoot.GetText().ToString();
        var document2AfterRoot = await document2After.GetSyntaxRootAsync();
        var document2AfterContent = document2AfterRoot.GetText().ToString();

        document2BeforeContent.Should().Be(document2AfterContent);
    }

    [TestMethod]
    [DataRow(FixAllScope.Project)]
    [DataRow(FixAllScope.Solution)]
    public async Task GetFixAsync_ForProjectAndSolution_AllFilesAreFixed(FixAllScope scope)
    {
        var codeFix = new DummyCodeFixCS();
        var project = CreateProject();
        var document1Before = project.FindDocument("MyFile1.cs");
        var document2Before = project.FindDocument("MyFile2.cs");
        var compilation = project.GetCompilation();
        var diagnostics = DiagnosticVerifier.AnalyzerDiagnostics(compilation, new DummyAnalyzerCS(), CompilationErrorBehavior.Ignore).ToImmutableArray();

        var fixAllContext = new FixAllContext(project.Project, codeFix, scope, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(diagnostics), default);
        var result = await SonarAnalyzer.Core.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext);
        var executedOperation = await result.GetOperationsAsync(default);
        var document1After = executedOperation.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document1Before.Id);
        var document2After = executedOperation.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document2Before.Id);

        var document1BeforeRoot = await document1Before.GetSyntaxRootAsync();
        var document1BeforeContent = document1BeforeRoot.GetText().ToString();
        var document1AfterRoot = await document1After.GetSyntaxRootAsync();
        var document1AfterContent = document1AfterRoot.GetText().ToString();

        document1BeforeContent.Should().NotBe(document1AfterContent);

        var document2BeforeRoot = await document2Before.GetSyntaxRootAsync();
        var document2BeforeContent = document2BeforeRoot.GetText().ToString();
        var document2AfterRoot = await document2After.GetSyntaxRootAsync();
        var document2AfterContent = document2AfterRoot.GetText().ToString();

        document2BeforeContent.Should().NotBe(document2AfterContent);
    }

    private static ProjectBuilder CreateProject() =>
        SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.CSharp)
            .AddSnippet(@"public class C1 { public void M1() { int number1 = 1 } };", "MyFile1.cs")
            .AddSnippet(@"public class C2 { public void M2() { int number2 = 2 } };", "MyFile2.cs");
}
