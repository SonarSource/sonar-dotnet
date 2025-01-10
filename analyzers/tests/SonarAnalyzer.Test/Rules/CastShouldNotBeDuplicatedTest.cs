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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CastShouldNotBeDuplicatedTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<CastShouldNotBeDuplicated>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void CastShouldNotBeDuplicated() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.cs").Verify();

#if NET

    [TestMethod]
    public void CastShouldNotBeDuplicated_CSharp9() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void CastShouldNotBeDuplicated_CSharp10() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void CastShouldNotBeDuplicated_CSharp11() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.CSharp11.cs")
            .WithOptions(LanguageOptions.FromCSharp11)
            .Verify();

    [TestMethod]
    public void CastShouldNotBeDuplicated_CSharp12() =>
        Builder.AddPaths("CastShouldNotBeDuplicated.CSharp12.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

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
