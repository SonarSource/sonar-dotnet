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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class SpecifyIFormatProviderOrCultureInfoTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<SpecifyIFormatProviderOrCultureInfo>();

        [TestMethod]
        public void SpecifyIFormatProviderOrCultureInfo() =>
            builder.AddPaths("SpecifyIFormatProviderOrCultureInfo.cs").Verify();

        [TestMethod]
        public void SpecifyIFormatProviderOrCultureInfo_BeforeCSharp13() =>
            builder.AddSnippet("""
                    using System;

                    class C
                    {
                        void M()
                        {
                            string.Format("bla");                                      // Noncompliant
                            string.Format("%s %s", "foo", "bar", "quix", "hi", "bye"); // Noncompliant
                        }
                    }
                    """)
                .WithOptions(LanguageOptions.BeforeCSharp13)
                .Verify();

#if NET

        [TestMethod]
        public void SpecifyIFormatProviderOrCultureInfo_CS_Latest() =>
            builder.AddPaths("SpecifyIFormatProviderOrCultureInfo.Latest.cs").Verify();

        // Repro https://sonarsource.atlassian.net/browse/NET-230
        [TestMethod]
        public void SpecifyIFormatProviderOrCultureInfo_FromCSharp13() =>
            builder.AddSnippet("""
                    using System;

                    string.Format("bla");                                      // FN
                    string.Format("%s %s", "foo", "bar", "quix", "hi", "bye"); // FN
                    """)
                .WithOptions(LanguageOptions.FromCSharp13)
                .WithTopLevelStatements()
                .VerifyNoIssues();

#endif

    }
}
