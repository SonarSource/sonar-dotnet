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

using SonarAnalyzer.TestFramework.MetadataReferences;

namespace SonarAnalyzer.CSharp.Styling.Test.Rules;

[TestClass]
public class UseRegexSafeIsMatchTest
{
    [TestMethod]
    public void UseRegexSafeIsMatch() =>
        StylingVerifierBuilder.Create<UseRegexSafeIsMatch>().AddPaths("UseRegexSafeIsMatch.cs").AddReferences(MetadataReferenceFacade.RegularExpressions).Verify();

    [TestMethod]
    public void UseRegexSafeIsMatchWithoutRegexExtensions() =>
        StylingVerifierBuilder.Create<UseRegexSafeIsMatch>()
            .AddSnippet("""
                            using System.Text.RegularExpressions;

                            class UseRegexSafeIsMatchNonCompliant
                            {
                                private Regex regex;
                                void InstanceRegex(string content)
                                {
                                    regex.IsMatch(content);         // Compliant
                                    regex.Matches(content);         // Compliant
                                    regex.Match(content);           // Compliant                                          
                                }
                            }
                            """)
            .AddReferences(MetadataReferenceFacade.RegularExpressions)
            .VerifyNoIssues();

    [TestMethod]
    public void UseRegexSafeIsMatchWithRegexExtensionOnlyIsMatch() =>
        StylingVerifierBuilder.Create<UseRegexSafeIsMatch>()
            .AddSnippet("""
                            using System.Text.RegularExpressions;
                            using System;
                            namespace RegexNamespace
                            {
                                class UseRegexSafeIsMatchNonCompliant
                                {
                                    private Regex regex;
                                    void InstanceRegex(string content)
                                    {
                                        regex.IsMatch(content);         // Noncompliant
                                        regex.Matches(content);         // Compliant
                                        regex.Match(content);           // Compliant                            
                                    }
                                }
                            }

                            namespace SafeRegexNamespace
                            {
                                public static class RegexExtensions
                                {
                                    public static bool SafeIsMatch(this Regex regex, string input) =>
                                        throw new NotImplementedException();
                                }
                            }
                            """)
            .AddReferences(MetadataReferenceFacade.RegularExpressions)
            .Verify();

    [TestMethod]
    public void UseRegexSafeIsMatchWithRegexExtensionOnlyMatch() =>
        StylingVerifierBuilder.Create<UseRegexSafeIsMatch>()
            .AddSnippet("""
                                using System.Text.RegularExpressions;
                                using System;
                                class UseRegexSafeIsMatchNonCompliant
                                {
                                    private Regex regex;
                                    void InstanceRegex(string content)
                                    {
                                        regex.IsMatch(content);         // Compliant 
                                        regex.Matches(content);         // Compliant
                                        regex.Match(content);           // Noncompliant                           
                                    }
                                }

                                public static class RegexExtensions
                                {
                                    public static bool SafeMatch(this Regex regex, string input) =>
                                        throw new NotImplementedException();
                                }
                                """)
            .AddReferences(MetadataReferenceFacade.RegularExpressions)
            .Verify();

    [TestMethod]
    public void UseRegexSafeIsMatchWithRegexExtensionOnlyMatches() =>
        StylingVerifierBuilder.Create<UseRegexSafeIsMatch>()
            .AddSnippet("""
                                using System.Text.RegularExpressions;
                                using System;
                                class UseRegexSafeIsMatchNonCompliant
                                {
                                    private Regex regex;
                                    void InstanceRegex(string content)
                                    {
                                        regex.IsMatch(content);         // Compliant
                                        regex.Matches(content);         // Noncompliant
                                        regex.Match(content);           // Compliant                            
                                    }
                                }

                                public static class RegexExtensions
                                {
                                    public static bool SafeMatches(this Regex regex, string input) =>
                                        throw new NotImplementedException();
                                }
                                """)
            .AddReferences(MetadataReferenceFacade.RegularExpressions)
            .Verify();
}
