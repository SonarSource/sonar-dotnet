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

namespace SonarAnalyzer.Test.TestFramework.Tests;

[TestClass]
public class ProjectBuilderTest
{
    private static readonly ProjectBuilder EmptyCS = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp);
    private static readonly ProjectBuilder EmptyVB = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic);

    [TestMethod]
    public void AddDocument_ValidExtension()
    {
        EmptyCS.AddDocument(@"TestCases\ProjectBuilder.AddDocument.cs").FindDocument("ProjectBuilder.AddDocument.cs").Should().NotBeNull();
        EmptyVB.AddDocument(@"TestCases\ProjectBuilder.AddDocument.vb").FindDocument("ProjectBuilder.AddDocument.vb").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_MismatchingExtension()
    {
        EmptyCS.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.vb")).Should().Throw<ArgumentException>();
        EmptyVB.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.cs")).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_InvalidExtension()
    {
        EmptyCS.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.unknown")).Should().Throw<ArgumentException>();
        EmptyVB.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.unknown")).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_SupportsRazorFiles()
    {
        EmptyCS.AddDocument(@"TestCases\ProjectBuilder.AddDocument.razor").FindDocument("ProjectBuilder.AddDocument.razor").Should().NotBeNull();
        EmptyVB.AddDocument(@"TestCases\ProjectBuilder.AddDocument.razor").FindDocument("ProjectBuilder.AddDocument.razor").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_CsharpSupportsCshtmlFiles() =>
        EmptyCS.AddDocument(@"TestCases\ProjectBuilder.AddDocument.cshtml").FindDocument("ProjectBuilder.AddDocument.cshtml").Should().NotBeNull();

    [TestMethod]
    public void AddDocument_VbnetDoesntSupportCshtmlFiles() =>
        EmptyVB.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.cshtml").FindDocument("ProjectBuilder.AddDocument.cshtml")).Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddDocument_CsharpDoesntSupportVbnetFiles() =>
        EmptyCS.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.vbhtml").FindDocument("ProjectBuilder.AddDocument.vbhtml").Should().NotBeNull()).Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddDocument_VbnetDoesntSupportVbnetFiles() =>
        EmptyVB.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.vbhtml").FindDocument("ProjectBuilder.AddDocument.vbhtml").Should().NotBeNull()).Should().Throw<ArgumentException>();
}
