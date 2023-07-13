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
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.UnitTest.AnalysisContext
{
    [TestClass]
    public class SonarCodeFixContextTest
    {
        [TestMethod]
        public void SonarCodeFixContext_Properties_ReturnRoslynCodeFixContextProperties()
        {
            // Arrange
            var document = CreateProject().FindDocument(Path.GetFileName("MyFile.cs"));
            var cancellationToken = new CancellationToken(true);
            var diagnosticDescriptor = new DiagnosticDescriptor("1", "title", "format", "category", DiagnosticSeverity.Hidden, false);
            var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);
            var sonarCodefix = new SonarCodeFixContext(new CodeFixContext(document, diagnostic, (_, _) => Console.WriteLine("Hello world"), cancellationToken));

            // Act & Assert
            sonarCodefix.CancellationToken.Should().Be(cancellationToken);
            sonarCodefix.Document.Should().Be(document);
            sonarCodefix.Diagnostics.Should().Contain(diagnostic);
            sonarCodefix.Span.Should().Be(new TextSpan(0, 0));
        }

        [TestMethod]
        public void SonarCodeFixContext_RegisterDocumentCodeFix_CodeFixRegistered()
        {
            // Arrange
            var isActionRegistered = false;
            var document = CreateProject().FindDocument(Path.GetFileName("MyFile.cs"));
            var cancellationToken = new CancellationToken(true);
            var diagnosticDescriptor = new DiagnosticDescriptor("1", "title", "format", "category", DiagnosticSeverity.Hidden, false);
            var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);
            var sonarCodefix = new SonarCodeFixContext(new CodeFixContext(document, diagnostic, RegisterCodeFixActionHandler, cancellationToken));

            // Act
            sonarCodefix.RegisterCodeFix("Title", _ => Task.FromResult(document), ImmutableArray.Create(diagnostic));

            // Assert
            isActionRegistered.Should().BeTrue();

            void RegisterCodeFixActionHandler(CodeAction action, ImmutableArray<Diagnostic> diagnostics) =>
                isActionRegistered = true;
        }

        [TestMethod]
        public void SonarCodeFixContext_RegisterSolutionCodeFix_CodeFixRegistered()
        {
            // Arrange
            var isActionRegistered = false;
            var document = CreateProject().FindDocument(Path.GetFileName("MyFile.cs"));
            var cancellationToken = new CancellationToken(true);
            var diagnosticDescriptor = new DiagnosticDescriptor("1", "title", "format", "category", DiagnosticSeverity.Hidden, false);
            var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);
            var sonarCodefix = new SonarCodeFixContext(new CodeFixContext(document, diagnostic, RegisterCodeFixActionHandler, cancellationToken));

            // Act
            sonarCodefix.RegisterCodeFix("Title", _ => Task.FromResult(new AdhocWorkspace().CurrentSolution), ImmutableArray.Create(diagnostic));

            // Assert
            isActionRegistered.Should().BeTrue();

            void RegisterCodeFixActionHandler(CodeAction action, ImmutableArray<Diagnostic> diagnostics) =>
                isActionRegistered = true;
        }

        private static ProjectBuilder CreateProject() =>
            SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddSnippet(@"Console.WriteLine(""Hello World"")", "MyFile.cs");
    }
}
