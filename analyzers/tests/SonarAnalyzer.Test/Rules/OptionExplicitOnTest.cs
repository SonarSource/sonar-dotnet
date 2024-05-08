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

using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class OptionExplicitOnTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<OptionExplicitOn>();

    [TestMethod]
    public void OptionExplicitOn_IsOffForProject()
    {
        var project = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).AddSnippet("' Noncompliant ^1#0 {{Configure 'Option Explicit On' for assembly 'project0'.}}");
        var options = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionExplicit: false);    // optionExplicit is true by default => tested in other tests
        var compilation = project.GetCompilation(null, options);
        DiagnosticVerifier.Verify(compilation, new OptionExplicitOn());
    }

    [TestMethod]
    public void OptionExplicitOn_IsOff() =>
        builder.AddSnippet("Option Explicit Off ' Noncompliant ^1#19 {{Change this to 'Option Explicit On'.}}").Verify();

    [TestMethod]
    public void OptionExplicitOn_IsOn() =>
        builder.AddSnippet("Option Explicit On").VerifyNoIssues();

    [TestMethod]
    public void OptionExplicitOn_IsMissing() =>
        builder.AddSnippet("Option Strict Off").VerifyNoIssues();

    [TestMethod]
    public void OptionExplicitOn_Concurrent()
    {
        using var scope = new EnvironmentVariableScope { EnableConcurrentAnalysis = true};
        var project = SolutionBuilder.Create()
                                     .AddProject(AnalyzerLanguage.VisualBasic)
                                     .AddSnippet("' Noncompliant ^1#0 {{Configure 'Option Explicit On' for assembly 'project0'.}}")
                                     .AddSnippet("Option Explicit Off ' Noncompliant ^1#19 {{Change this to 'Option Explicit On'.}}");
        var options = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionExplicit: false);    // optionExplicit is true by default => tested in other tests
        var compilation = project.GetCompilation(null, options);
        DiagnosticVerifier.Verify(compilation, new OptionExplicitOn());
    }
}
