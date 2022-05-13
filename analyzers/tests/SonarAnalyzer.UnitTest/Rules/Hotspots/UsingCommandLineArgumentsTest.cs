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

using SonarAnalyzer.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UsingCommandLineArgumentsTest
    {
        [TestMethod]
        public void UsingCommandLineArguments_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\Hotspots\UsingCommandLineArguments.cs",
                                    new CS.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void UsingCommandLineArguments_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\Hotspots\UsingCommandLineArguments.CSharp9.cs",
                                                      new CS.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void UsingCommandLineArguments_CS_Partial()
        {
            const string code1 = @"
partial class Program1
{
    static partial void Main(params string[] args) // Noncompliant
    {
        System.Console.WriteLine(args);
    }
}";
            const string code2 = @"
partial class Program1
{
    static partial void Main(params string[] args); // Compliant, we raise only on methods with implementation
}";
            var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippets(code1, code2).GetCompilation();
            DiagnosticVerifier.Verify(compilation, new CS.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled), CompilationErrorBehavior.Default);
        }

        [TestMethod]
        public void UsingCommandLineArguments_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\Hotspots\UsingCommandLineArguments.vb",
                                    new VB.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void UsingCommandLineArguments_VB_Partial()
        {
            const string code1 = @"
Partial Class Program1
    Private Shared Sub Main(ParamArray args As String()) ' Noncompliant
        System.Console.WriteLine(args)
    End Sub
End Class";
            const string code2 = @"
Partial Class Program1
    Private Shared Partial Sub Main(ParamArray args As String()) ' Compliant, we raise only on methods with implementation
    End Sub
End Class";
            var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).AddSnippets(code1, code2).GetCompilation();
            DiagnosticVerifier.Verify(compilation, new VB.UsingCommandLineArguments(AnalyzerConfiguration.AlwaysEnabled), CompilationErrorBehavior.Default);
        }
    }
}
