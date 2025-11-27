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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CastShouldNotBeDuplicatedTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<CastShouldNotBeDuplicated>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void CastShouldNotBeDuplicated() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.cs").Verify();

    [TestMethod]
    public void CastShouldNotBeDuplicated_CSharpLatest() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#if NET

    [TestMethod]
    public void CastShouldNotBeDuplicated_MvcView() =>
        Builder
        .AddSnippet("""
            public class Base {}
            public class Derived: Base
            {
                public int Prop { get; set; }
            }
            """)
        .AddPaths("CastShouldNotBeDuplicated.cshtml")
        .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
        .VerifyNoIssues();

#endif

}
