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
using SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class OptionStrictOnTest
    {
        [TestMethod]
        public void OptionStrictOn_IsOff_ForProject() =>
            VerifyAnalyzer("' Noncompliant ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}", OptionStrict.Off);

        [TestMethod]
        public void OptionStrictOn_IsCustom_ForProject() =>
            VerifyAnalyzer("' Noncompliant ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}", OptionStrict.Custom);

        [TestMethod]
        public void OptionStrictOn_IsOff() =>
            VerifyAnalyzer("Option Strict Off ' Noncompliant ^1#17 {{Change this to 'Option Strict On'.}}", OptionStrict.On);

        [TestMethod]
        public void OptionStrictOn_IsOn() =>
            VerifyAnalyzer("Option Strict On ' Compliant", OptionStrict.On);

        [TestMethod]
        public void OptionStrictOn_Concurrent()
        {
            using var scope = new EnvironmentVariableScope { EnableConcurrentAnalysis = true};
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
            DiagnosticVerifier.Verify(compilation, new OptionStrictOn());
        }
    }
}
