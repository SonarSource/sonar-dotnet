/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.VisualBasic;
using SonarAnalyzer.UnitTest.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class OptionStrictOnTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void OptionStrictOn_IsOff_ForProject() =>
            VerifyAnalyzer("' Noncompliant ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}", OptionStrict.Off);

        [TestMethod]
        [TestCategory("Rule")]
        public void OptionStrictOn_IsCustom_ForProject() =>
            VerifyAnalyzer("' Noncompliant ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}", OptionStrict.Custom);

        [TestMethod]
        [TestCategory("Rule")]
        public void OptionStrictOn_IsOff() =>
            VerifyAnalyzer("Option Strict Off ' Noncompliant ^1#17 {{Change this to 'Option Strict On'.}}", OptionStrict.On);

        [TestMethod]
        [TestCategory("Rule")]
        public void OptionStrictOn_IsOn() =>
            VerifyAnalyzer("Option Strict On ' Compliant", OptionStrict.On);

        [TestMethod]
        [TestCategory("Rule")]
        public void OptionStrictOn_Concurrent()
        {
            using var _ = new EnvironmentVariableScope { EnableConcurrentAnalysis = true};
            var project = SolutionBuilder.Create()
                                         .AddProject(AnalyzerLanguage.VisualBasic)
                                         .AddSnippet("Option Strict Off ' Noncompliant ^1#17 {{Change this to 'Option Strict On'.}}")
                                         .AddSnippet("Option Strict On ' Compliant");
            VerifyAnalyzer(project, OptionStrict.On);
        }

        private static void VerifyAnalyzer(string snippet, OptionStrict optionStrict)
        {
            var project = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).AddSnippet(snippet);
            VerifyAnalyzer(project, optionStrict);
        }

        private static void VerifyAnalyzer(ProjectBuilder project, OptionStrict optionStrict)
        {
            var options = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionStrict: optionStrict);
            var compilation = project.GetCompilation(null, options);
            DiagnosticVerifier.Verify(compilation, new OptionStrictOn(), CompilationErrorBehavior.Default);
        }
    }
}
