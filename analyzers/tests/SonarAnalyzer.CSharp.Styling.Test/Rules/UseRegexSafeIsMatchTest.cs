/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
