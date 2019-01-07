/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
extern alias vbnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharp = csharp::SonarAnalyzer.Rules.CSharp;
using VisualBasic = vbnet::SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UsingCommandLineArgumentsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCommandLineArguments_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UsingCommandLineArguments.cs",
                new CSharp.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCommandLineArguments_CS_Partial()
        {
            var compilation = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.CSharp)
                .AddSnippet(@"
partial class Program1
{
    static partial void Main(params string[] args) // Noncompliant
    {
        System.Console.WriteLine(args);
    }
}")
                .AddSnippet(@"
partial class Program1
{
    static partial void Main(params string[] args); // Compliant, we raise only on methods with implementation
}")
                .GetCompilation();

            DiagnosticVerifier.Verify(
                compilation,
                new CSharp.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled),
                CompilationErrorBehavior.Default);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCommandLineArguments_CS_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\UsingCommandLineArguments.cs",
                new CSharp.UsingCommandLineArguments());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCommandLineArguments_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UsingCommandLineArguments.vb",
                new VisualBasic.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCommandLineArguments_VB_Partial()
        {
            var compilation = SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.VisualBasic)
                .AddSnippet(@"
Partial Class Program1
    Private Shared Sub Main(ParamArray args As String()) ' Noncompliant
        System.Console.WriteLine(args)
    End Sub
End Class
")
                .AddSnippet(@"
Partial Class Program1
    Private Shared Partial Sub Main(ParamArray args As String()) ' Compliant, we raise only on methods with implementation
    End Sub
End Class
")
                .GetCompilation();

            DiagnosticVerifier.Verify(
                compilation,
                new VisualBasic.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled),
                CompilationErrorBehavior.Default);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCommandLineArguments_VB_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\UsingCommandLineArguments.vb",
                new VisualBasic.UsingCommandLineArguments());
        }
    }
}
