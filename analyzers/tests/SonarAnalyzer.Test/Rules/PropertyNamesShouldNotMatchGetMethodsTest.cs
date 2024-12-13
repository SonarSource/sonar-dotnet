/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
public class PropertyNamesShouldNotMatchGetMethodsTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<PropertyNamesShouldNotMatchGetMethods>();

    [TestMethod]
    public void PropertyNamesShouldNotMatchGetMethods() =>
        builder.AddPaths("PropertyNamesShouldNotMatchGetMethods.cs").Verify();

    [TestMethod]
    public void PropertyNamesShouldNotMatchGetMethods_InvalidCode() =>
        builder.AddSnippet("""
            public class Sample
            {
                // Missing identifier on purpose
                public int { get; }
                public int () { return 42; }
            }
            """).VerifyNoIssuesIgnoreErrors();

#if NET
    [TestMethod]
    public void PropertyNamesShouldNotMatchGetMethods_Latest() =>
        builder
            .AddPaths(
                "PropertyNamesShouldNotMatchGetMethods.Latest.cs",
                "PropertyNamesShouldNotMatchGetMethods.Latest.Partial1.g.cs",
                "PropertyNamesShouldNotMatchGetMethods.Latest.Partial2.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
#endif

}
