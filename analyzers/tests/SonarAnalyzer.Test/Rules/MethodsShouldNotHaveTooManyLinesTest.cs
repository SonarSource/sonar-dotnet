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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MethodsShouldNotHaveTooManyLinesTest
{
    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_DefaultValues_CS() =>
        new VerifierBuilder<CS.MethodsShouldNotHaveTooManyLines>().AddPaths("MethodsShouldNotHaveTooManyLines_DefaultValues.cs").Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_CustomValues_CS() =>
        CreateCSBuilder(2).AddPaths("MethodsShouldNotHaveTooManyLines_CustomValues.cs").Verify();

#if NET
    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_LocalFunctions() =>
        CreateCSBuilder(5).AddPaths("MethodsShouldNotHaveTooManyLines.LocalFunctions.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_LocalFunctions_CSharp9() =>
        CreateCSBuilder(5).AddPaths("MethodsShouldNotHaveTooManyLines.LocalFunctions.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_CustomValues_CSharp9() =>
        CreateCSBuilder(2).AddPaths("MethodsShouldNotHaveTooManyLines_CustomValues.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_CustomValues_CSharp10() =>
        CreateCSBuilder(2).AddPaths("MethodsShouldNotHaveTooManyLines_CustomValues.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_CSharp9_NoUsing() =>
        CreateCSBuilder(2).AddSnippet(@"
int i = 1; i++;

void LocalFunction() // Noncompliant {{This local function has 4 lines, which is greater than the 2 lines authorized.}}
{
i++;
i++;
i++;
i++;
}")
        .WithOptions(LanguageOptions.FromCSharp9)
        .WithOutputKind(OutputKind.ConsoleApplication)
        .Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_CSharp9_Valid() =>
        CreateCSBuilder(4)
            .AddSnippet("""
                int i = 1; i++;
                i++;
                i++;
                i++;
                """)
            .WithOptions(LanguageOptions.FromCSharp9)
            .WithOutputKind(OutputKind.ConsoleApplication)
            .VerifyNoIssues();
#endif

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_DoesntReportInTest_CS() =>
        new VerifierBuilder<CS.MethodsShouldNotHaveTooManyLines>().AddPaths("MethodsShouldNotHaveTooManyLines_DefaultValues.cs")
        .AddTestReference()
        .VerifyNoIssues();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_InvalidSyntax_CS() =>
        CreateCSBuilder(2)
            .AddSnippet("""
                public class Foo
                {
                    public string ()
                    {
                        return "f";
                    }
                }
                """)
            .VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    [DataRow(1)]
    [DataRow(0)]
    [DataRow(-1)]
    public void MethodsShouldNotHaveTooManyLines_InvalidMaxThreshold_CS(int max)
    {
        var compilation = SolutionBuilder.CreateSolutionFromPath(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.cs")
            .Compile(LanguageOptions.CSharpLatest.ToArray()).Single();
        var errors = DiagnosticVerifier.AnalyzerExceptions(compilation, new CS.MethodsShouldNotHaveTooManyLines { Max = max });
        errors.Should().OnlyContain(x => x.GetMessage(null).Contains("Invalid rule parameter: maximum number of lines = ")).And.HaveCount(12);
    }

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_DefaultValues_VB() =>
        new VerifierBuilder<VB.MethodsShouldNotHaveTooManyLines>().AddPaths("MethodsShouldNotHaveTooManyLines_DefaultValues.vb").Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_CustomValues_VB() =>
        new VerifierBuilder().AddAnalyzer(() => new VB.MethodsShouldNotHaveTooManyLines { Max = 2 })
        .AddPaths("MethodsShouldNotHaveTooManyLines_CustomValues.vb")
        .Verify();

    [TestMethod]
    public void MethodsShouldNotHaveTooManyLines_DoesntReportInTest_VB() =>
        new VerifierBuilder<VB.MethodsShouldNotHaveTooManyLines>().AddPaths("MethodsShouldNotHaveTooManyLines_DefaultValues.vb")
        .AddTestReference()
        .VerifyNoIssues();

    [TestMethod]
    [DataRow(1)]
    [DataRow(0)]
    [DataRow(-1)]
    public void MethodsShouldNotHaveTooManyLines_InvalidMaxThreshold_VB(int max)
    {
        var compilation = SolutionBuilder.CreateSolutionFromPath(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.vb")
            .Compile(LanguageOptions.VisualBasicLatest.ToArray()).Single();
        var errors = DiagnosticVerifier.AnalyzerExceptions(compilation, new VB.MethodsShouldNotHaveTooManyLines { Max = max });
        errors.Should().OnlyContain(x => x.GetMessage(null).Contains("Invalid rule parameter: maximum number of lines = ")).And.HaveCount(7);
    }

    private static VerifierBuilder CreateCSBuilder(int maxLines) =>
        new VerifierBuilder().AddAnalyzer(() => new CS.MethodsShouldNotHaveTooManyLines { Max = maxLines });
}
