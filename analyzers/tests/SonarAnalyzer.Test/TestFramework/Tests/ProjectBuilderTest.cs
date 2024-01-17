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
        EmptyCS.AddDocument("TestCases\\VariableUnused.cs").FindDocument("VariableUnused.cs").Should().NotBeNull();
        EmptyVB.AddDocument("TestCases\\VariableUnused.vb").FindDocument("VariableUnused.vb").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_MismatchingExtension()
    {
        Action f;

        f = () => EmptyCS.AddDocument("TestCases\\VariableUnused.vb");
        f.Should().Throw<ArgumentException>();

        f = () => EmptyVB.AddDocument("TestCases\\VariableUnused.cs");
        f.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_InvalidExtension()
    {
        Action f;

        f = () => EmptyCS.AddDocument("TestCases\\VariableUnused.unknown");
        f.Should().Throw<ArgumentException>();

        f = () => EmptyVB.AddDocument("TestCases\\VariableUnused.unknown");
        f.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_SupportsRazorFiles()
    {
        EmptyCS.AddDocument("TestCases\\UnusedPrivateMember.razor").FindDocument("UnusedPrivateMember.razor").Should().NotBeNull();
        EmptyVB.AddDocument("TestCases\\UnusedPrivateMember.razor").FindDocument("UnusedPrivateMember.razor").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_CsharpSupportsCshtmlFiles() =>
        EmptyCS.AddDocument("TestCases\\UnusedPrivateMember.cshtml").FindDocument("UnusedPrivateMember.cshtml").Should().NotBeNull();

    [TestMethod]
    public void AddDocument_VbnetDoesntSupportCshtmlFiles() =>
        new Action(() => EmptyVB.AddDocument("TestCases\\UnusedPrivateMember.cshtml").FindDocument("UnusedPrivateMember.cshtml").Should().NotBeNull())
            .Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddDocument_CsharpDoesntSupportVbnetFiles() =>
        new Action(() => EmptyCS.AddDocument("TestCases\\UnusedPrivateMember.vbhtml").FindDocument("UnusedPrivateMember.vbhtml").Should().NotBeNull())
            .Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddDocument_VbnetDoesntSupportVbnetFiles() =>
        new Action(() => EmptyVB.AddDocument("TestCases\\UnusedPrivateMember.vbhtml").FindDocument("UnusedPrivateMember.vbhtml").Should().NotBeNull())
            .Should().Throw<ArgumentException>();
}
