/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class OptionExplicitOnTest
{
    [TestMethod]
    public void OptionExplicitOn_IsOffForProject() =>
        CreateBuilder("' Noncompliant ^1#0 {{Configure 'Option Explicit On' for assembly 'project0'.}}", false).Verify();

    [TestMethod]
    public void OptionExplicitOn_IsOff() =>
        CreateBuilder("Option Explicit Off ' Noncompliant ^1#19 {{Change this to 'Option Explicit On'.}}", true).Verify();

    [TestMethod]
    public void OptionExplicitOn_IsOn() =>
        CreateBuilder("Option Explicit On", true).VerifyNoIssues();

    [TestMethod]
    public void OptionExplicitOn_IsMissing() =>
        CreateBuilder("Option Strict Off", true).VerifyNoIssues();

    [TestMethod]
    public void OptionExplicitOn_Concurrent()  =>
        CreateBuilder(false)
            .AddSnippet("' Noncompliant ^1#0 {{Configure 'Option Explicit On' for assembly 'project0'.}}")
            .AddSnippet("Option Explicit Off ' Noncompliant ^1#19 {{Change this to 'Option Explicit On'.}}")
            .WithConcurrentAnalysis(true)
            .Verify();

    private static VerifierBuilder CreateBuilder(string snippet, bool optionExplicit) =>
        CreateBuilder(optionExplicit).AddSnippet(snippet);

    private static VerifierBuilder CreateBuilder(bool optionExplicit) =>
        new VerifierBuilder<OptionExplicitOn>().WithCompilationOptionsCustomization(x => ((VisualBasicCompilationOptions)x).WithOptionExplicit(optionExplicit));
}
