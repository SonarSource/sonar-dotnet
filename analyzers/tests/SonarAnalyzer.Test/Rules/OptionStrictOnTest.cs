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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class OptionStrictOnTest
    {
        [TestMethod]
        public void OptionStrictOn_IsOff_ForProject() =>
            CreateBuilder("' Noncompliant ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}", OptionStrict.Off).Verify();

        [TestMethod]
        public void OptionStrictOn_IsCustom_ForProject() =>
            CreateBuilder("' Noncompliant ^1#0 {{Configure 'Option Strict On' for assembly 'project0'.}}", OptionStrict.Custom).Verify();

        [TestMethod]
        public void OptionStrictOn_IsOff() =>
            CreateBuilder("Option Strict Off ' Noncompliant ^1#17 {{Change this to 'Option Strict On'.}}", OptionStrict.On).Verify();

        [TestMethod]
        public void OptionStrictOn_IsOn() =>
            CreateBuilder("Option Strict On ' Compliant", OptionStrict.On).VerifyNoIssues();

        [TestMethod]
        public void OptionStrictOn_Concurrent() =>
            CreateBuilder(OptionStrict.On)
                .AddSnippet("Option Strict Off ' Noncompliant ^1#17 {{Change this to 'Option Strict On'.}}")
                .AddSnippet("Option Strict On ' Compliant")
                .WithConcurrentAnalysis(true)
                .Verify();

        private static VerifierBuilder CreateBuilder(string snippet, OptionStrict optionStrict) =>
            CreateBuilder(optionStrict).AddSnippet(snippet);

        private static VerifierBuilder CreateBuilder(OptionStrict optionStrict) =>
            new VerifierBuilder<OptionStrictOn>().WithCompilationOptionsCustomization(x => ((VisualBasicCompilationOptions)x).WithOptionStrict(optionStrict));
    }
}
