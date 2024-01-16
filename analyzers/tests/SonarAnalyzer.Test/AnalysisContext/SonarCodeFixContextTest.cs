/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Test.AnalysisContext
{
    [TestClass]
    public class SonarCodeFixContextTest
    {
        [TestMethod]
        public async Task SonarCodeFixContext_Properties_ReturnRoslynCodeFixContextProperties()
        {
            // Arrange
            var document = CreateProject().FindDocument("MyFile.cs");
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var literal = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<LiteralExpressionSyntax>().Single();
            var cancellationToken = new CancellationToken(true);
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("1", "title", "format", "category", DiagnosticSeverity.Hidden, false), literal.GetLocation());
            var sonarCodefix = new SonarCodeFixContext(new CodeFixContext(document, diagnostic, (_, _) => { }, cancellationToken));

            // Act & Assert
            sonarCodefix.CancellationToken.Should().Be(cancellationToken);
            sonarCodefix.Document.Should().Be(document);
            sonarCodefix.Diagnostics.Should().Contain(diagnostic);
            sonarCodefix.Span.Should().Be(new TextSpan(18, 13));
        }

        [TestMethod]
        public void SonarCodeFixContext_RegisterDocumentCodeFix_CodeFixRegistered()
        {
            // Arrange
            var isActionRegistered = false;
            var document = CreateProject().FindDocument("MyFile.cs");
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("1", "title", "format", "category", DiagnosticSeverity.Hidden, false), Location.None);
            var sonarCodefix = new SonarCodeFixContext(new CodeFixContext(document, diagnostic, (_, _) => isActionRegistered = true, CancellationToken.None));

            // Act
            sonarCodefix.RegisterCodeFix("Title", _ => Task.FromResult(document), ImmutableArray.Create(diagnostic));

            // Assert
            isActionRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void SonarCodeFixContext_RegisterSolutionCodeFix_CodeFixRegistered()
        {
            // Arrange
            var isActionRegistered = false;
            var document = CreateProject().FindDocument("MyFile.cs");
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("1", "title", "format", "category", DiagnosticSeverity.Hidden, false), Location.None);
            var sonarCodefix = new SonarCodeFixContext(new CodeFixContext(document, diagnostic, (_, _) => isActionRegistered = true, CancellationToken.None));

            // Act
            sonarCodefix.RegisterCodeFix("Title", _ => Task.FromResult(new AdhocWorkspace().CurrentSolution), ImmutableArray.Create(diagnostic));

            // Assert
            isActionRegistered.Should().BeTrue();
        }

        private static ProjectBuilder CreateProject() =>
            SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddSnippet(@"Console.WriteLine(""Hello World"")", "MyFile.cs");
    }
}
