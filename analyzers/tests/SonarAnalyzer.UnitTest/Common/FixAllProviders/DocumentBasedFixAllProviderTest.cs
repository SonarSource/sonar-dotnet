/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.IO;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.UnitTest.TestFramework.Tests;

namespace SonarAnalyzer.UnitTest.Common.FixAllProviders
{
    [TestClass]
    public class DocumentBasedFixAllProviderTest
    {
        [DataTestMethod]
        [DataRow(FixAllScope.Document, "Fix all 'SDummy' in 'MyFile1.cs'")]
        [DataRow(FixAllScope.Project, "Fix all 'SDummy' in 'project0'")]
        [DataRow(FixAllScope.Solution, "Fix all 'SDummy' in Solution")]
        public void GetFixAsync_ForSupportedScope_HasCorrectTitle(FixAllScope scope, string expectedTitle)
        {
            var codeFix = new DummyCodeFixCS();
            var document = CreateProject().FindDocument(Path.GetFileName("MyFile1.cs"));
            var fixAllContext = new FixAllContext(document, codeFix, scope, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(null), default);
            var result = SonarAnalyzer.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext).Result;
            result.Title.Should().Be(expectedTitle);
        }

        [TestMethod]
        public void GetFixAsync_ForUnsupportedScope_ReturnsNull()
        {
            var codeFix = new DummyCodeFixCS();
            var document = CreateProject().FindDocument(Path.GetFileName("MyFile1.cs"));
            var fixAllContext = new FixAllContext(document, codeFix, FixAllScope.Custom, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(null), default);
            var result = SonarAnalyzer.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext).Result;
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetFixAsync_ForDocument_OnlyTheDocumentChanged()
        {
            var codeFix = new DummyCodeFixCS();
            var project = CreateProject();
            var document1Before = project.FindDocument(Path.GetFileName("MyFile1.cs"));
            var document2Before = project.FindDocument(Path.GetFileName("MyFile2.cs"));
            var compilation = project.GetCompilation();
            var diagnostics = DiagnosticVerifier.GetDiagnosticsNoExceptions(compilation, new DummyAnalyzerCS(), CompilationErrorBehavior.Ignore).ToImmutableArray();

            var fixAllContext = new FixAllContext(document1Before, codeFix, FixAllScope.Document, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(diagnostics), default);
            var result = SonarAnalyzer.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext).Result;
            var executedOperation = result.GetOperationsAsync(default).Result;
            var document1After = executedOperation.OfType<ApplyChangesOperation>().First().ChangedSolution.GetDocument(document1Before.Id);
            var document2After = executedOperation.OfType<ApplyChangesOperation>().First().ChangedSolution.GetDocument(document2Before.Id);

            var document1BeforeContent = document1Before.GetSyntaxRootAsync().Result.GetText().ToString();
            var document1AfterContent = document1After.GetSyntaxRootAsync().Result.GetText().ToString();
            document1BeforeContent.Should().NotBe(document1AfterContent);

            var document2BeforeContent = document2Before.GetSyntaxRootAsync().Result.GetText().ToString();
            var document2AfterContent = document2After.GetSyntaxRootAsync().Result.GetText().ToString();
            document2BeforeContent.Should().Be(document2AfterContent);
        }

        [DataTestMethod]
        [DataRow(FixAllScope.Project)]
        [DataRow(FixAllScope.Solution)]
        public void GetFixAsync_ForProjectAndSolution_AllFilesAreFixed(FixAllScope scope)
        {
            var codeFix = new DummyCodeFixCS();
            var project = CreateProject();
            var document1Before = project.FindDocument(Path.GetFileName("MyFile1.cs"));
            var document2Before = project.FindDocument(Path.GetFileName("MyFile2.cs"));
            var compilation = project.GetCompilation();
            var diagnostics = DiagnosticVerifier.GetDiagnosticsNoExceptions(compilation, new DummyAnalyzerCS(), CompilationErrorBehavior.Ignore).ToImmutableArray();

            var fixAllContext = new FixAllContext(project.Project, codeFix, scope, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(diagnostics), default);
            var result = SonarAnalyzer.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext).Result;
            var executedOperation = result.GetOperationsAsync(default).Result;
            var document1After = executedOperation.OfType<ApplyChangesOperation>().First().ChangedSolution.GetDocument(document1Before.Id);
            var document2After = executedOperation.OfType<ApplyChangesOperation>().First().ChangedSolution.GetDocument(document2Before.Id);

            var document1BeforeContent = document1Before.GetSyntaxRootAsync().Result.GetText().ToString();
            var document1AfterContent = document1After.GetSyntaxRootAsync().Result.GetText().ToString();
            document1BeforeContent.Should().NotBe(document1AfterContent);

            var document2BeforeContent = document2Before.GetSyntaxRootAsync().Result.GetText().ToString();
            var document2AfterContent = document2After.GetSyntaxRootAsync().Result.GetText().ToString();
            document2BeforeContent.Should().NotBe(document2AfterContent);
        }

        private static ProjectBuilder CreateProject() =>
            SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp, false)
                .AddSnippet(@"int number1 = 1;", "MyFile1.cs")
                .AddSnippet(@"int number2 = 2;", "MyFile2.cs");
    }
}
