/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MethodsShouldNotHaveTooManyLinesTest
    {
        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_DefaultValues_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.cs", new CS.MethodsShouldNotHaveTooManyLines());

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_CustomValues_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.cs", new CS.MethodsShouldNotHaveTooManyLines { Max = 2 });

#if NET
        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_CustomValues_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.CSharp9.cs", new CS.MethodsShouldNotHaveTooManyLines { Max = 2 });

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_CustomValues_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.CSharp10.cs", new CS.MethodsShouldNotHaveTooManyLines { Max = 2 });

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_CSharp9_NoUsing() =>
            OldVerifier.VerifyCSharpAnalyzer(
                @"
int i = 1; i++;

void LocalFunction() // Noncompliant {{This top level local function has 4 lines, which is greater than the 2 lines authorized.}}
{
    i++;
    i++;
    i++;
    i++;
}",
                new CS.MethodsShouldNotHaveTooManyLines { Max = 2 },
                ParseOptionsHelper.FromCSharp9,
                outputKind: OutputKind.ConsoleApplication);

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_CSharp9_Valid() =>
            OldVerifier.VerifyCSharpAnalyzer(@"
int i = 1; i++;
i++;
i++;
i++;",
                new CS.MethodsShouldNotHaveTooManyLines { Max = 4 },
                ParseOptionsHelper.FromCSharp9,
                outputKind: OutputKind.ConsoleApplication);
#endif

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_DoesntReportInTest_CS() =>
            OldVerifier.VerifyNoIssueReportedInTest(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.cs", new CS.MethodsShouldNotHaveTooManyLines());

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_InvalidSyntax_CS() =>
            OldVerifier.VerifyCSharpAnalyzer(@"
public class Foo
{
    public string ()
    {
        return ""f"";
    }
}",
                new CS.MethodsShouldNotHaveTooManyLines { Max = 2 }, CompilationErrorBehavior.Ignore);

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(0)]
        [DataRow(-1)]
        public void MethodsShouldNotHaveTooManyLines_InvalidMaxThreshold_CS(int max)
        {
            var compilation = SolutionBuilder.CreateSolutionFromPath(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.cs")
                .Compile(ParseOptionsHelper.CSharpLatest.ToArray()).Single();
            var diagnostics = DiagnosticVerifier.GetDiagnosticsIgnoreExceptions(compilation, new CS.MethodsShouldNotHaveTooManyLines { Max = max });
            diagnostics.Should().OnlyContain(x => x.Id == "AD0001" && x.GetMessage(null).Contains("Invalid rule parameter: maximum number of lines = ")).And.HaveCount(12);
        }

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_DefaultValues_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.vb", new VB.MethodsShouldNotHaveTooManyLines());

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_CustomValues_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.vb", new VB.MethodsShouldNotHaveTooManyLines { Max = 2 });

        [TestMethod]
        public void MethodsShouldNotHaveTooManyLines_DoesntReportInTest_VB() =>
            OldVerifier.VerifyNoIssueReportedInTest(@"TestCases\MethodsShouldNotHaveTooManyLines_DefaultValues.vb", new VB.MethodsShouldNotHaveTooManyLines());

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(0)]
        [DataRow(-1)]
        public void MethodsShouldNotHaveTooManyLines_InvalidMaxThreshold_VB(int max)
        {
            var compilation = SolutionBuilder.CreateSolutionFromPath(@"TestCases\MethodsShouldNotHaveTooManyLines_CustomValues.vb")
                .Compile(ParseOptionsHelper.VisualBasicLatest.ToArray()).Single();
            var diagnostics = DiagnosticVerifier.GetDiagnosticsIgnoreExceptions(compilation, new VB.MethodsShouldNotHaveTooManyLines { Max = max });
            diagnostics.Should().OnlyContain(x => x.Id == "AD0001" && x.GetMessage(null).Contains("Invalid rule parameter: maximum number of lines = ")).And.HaveCount(7);
        }
    }
}
