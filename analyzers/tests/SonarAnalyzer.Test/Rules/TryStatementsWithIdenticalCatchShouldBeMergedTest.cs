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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class TryStatementsWithIdenticalCatchShouldBeMergedTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<TryStatementsWithIdenticalCatchShouldBeMerged>();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TryStatementsWithIdenticalCatchShouldBeMerged() =>
            builder.AddPaths("TryStatementsWithIdenticalCatchShouldBeMerged.cs").Verify();

#if NET

        [TestMethod]
        public void TryStatementsWithIdenticalCatchShouldBeMerged_RazorFile_CorrectMessage() =>
            builder
                .AddSnippet(
                    """
                    @using System;
                    @code
                    {
                        public void Method()
                        {
                            try { }
                            catch (Exception)
                            {
                            }
                            finally { }

                            try { } // Noncompliant {{Combine this 'try' with the one starting on line 6.}}
                            catch (Exception)
                            {
                            }
                            finally { }
                        }
                    }
                    """,
                    "SomeRazorFile.razor")
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .Verify();

#endif

    }
}
