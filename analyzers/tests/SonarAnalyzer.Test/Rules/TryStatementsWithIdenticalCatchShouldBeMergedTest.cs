/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

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
            builder.AddSnippet(
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
