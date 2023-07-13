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
using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.UnitTest.TestFramework.Tests;

namespace SonarAnalyzer.UnitTest.Common.FixAllProviders
{
    [TestClass]
    public class DocumentBasedFixAllProviderTest
    {
        [DataTestMethod]
        [DataRow(FixAllScope.Document, "Fix all 'SDummy' in 'MyFile.cs'")]
        [DataRow(FixAllScope.Project, "Fix all 'SDummy' in 'project0'")]
        [DataRow(FixAllScope.Solution, "Fix all 'SDummy' in Solution")]
        public void GetFixAsync_DifferentScope_HasCorrectTitle(FixAllScope scope, string expectedTitle)
        {
            var codeFix = new DummyCodeFixCS();
            var document = CreateProject().FindDocument(Path.GetFileName("MyFile.cs"));
            var fixAllContext = new FixAllContext(document, codeFix, scope, "Dummy Action", codeFix.FixableDiagnosticIds, new FixAllDiagnosticProvider(null), default);
            var result = SonarAnalyzer.Common.DocumentBasedFixAllProvider.Instance.GetFixAsync(fixAllContext).Result;
            result.Title.Should().Be(expectedTitle);
        }

        private static ProjectBuilder CreateProject() =>
            SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddSnippet(@"Console.WriteLine(""Hello World"")", "MyFile.cs");
    }
}
