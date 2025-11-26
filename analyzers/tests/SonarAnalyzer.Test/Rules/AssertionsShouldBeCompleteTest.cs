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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class AssertionsShouldBeCompleteTest
{
    private readonly VerifierBuilder fluentAssertions = new VerifierBuilder<AssertionsShouldBeComplete>()
        .AddReferences(NuGetMetadataReference.FluentAssertions("6.10.0"))
        .AddReferences(MetadataReferenceFacade.SystemXml)
        .AddReferences(MetadataReferenceFacade.SystemXmlLinq)
        .AddReferences(MetadataReferenceFacade.SystemNetHttp)
        .AddReferences(MetadataReferenceFacade.SystemData);

    private readonly VerifierBuilder nfluent = new VerifierBuilder<AssertionsShouldBeComplete>()
        .AddReferences(NuGetMetadataReference.NFluent("2.8.0"));

    private readonly VerifierBuilder nsubstitute = new VerifierBuilder<AssertionsShouldBeComplete>()
        .AddReferences(NuGetMetadataReference.NSubstitute("5.0.0"));

    private readonly VerifierBuilder allFrameworks;

    public AssertionsShouldBeCompleteTest()
    {
        allFrameworks = new VerifierBuilder<AssertionsShouldBeComplete>()
            .AddReferences(fluentAssertions.References)
            .AddReferences(nfluent.References)
            .AddReferences(nsubstitute.References);
    }

    [TestMethod]
    public void AssertionsShouldBeComplete_FluentAssertions_CSharp7() =>
        fluentAssertions
        // The overload resolution errors for s[0].Should() and collection.Should() are fixed in CSharp 7.3.
        .WithOptions(ImmutableArray.Create<ParseOptions>(new CSharpParseOptions[] { new(LanguageVersion.CSharp7), new(LanguageVersion.CSharp7_1), new(LanguageVersion.CSharp7_2) }))
        .AddPaths("AssertionsShouldBeComplete.FluentAssertions.CSharp7.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_FluentAssertions_MissingParen() =>
        fluentAssertions
            .AddSnippet("""
                using FluentAssertions;
                public class Test
                {
                    public void MissingParen()
                    {
                        var s = "Test";
                        s.Should(;  // Error [CS1026]
                    }
                }
                """)
            .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_FluentAssertions_CSharpLatest() =>
        fluentAssertions
        .WithOptions(LanguageOptions.CSharpLatest)
        .AddPaths("AssertionsShouldBeComplete.FluentAssertions.Latest.cs")
        .WithConcurrentAnalysis(false)
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_NFluent_CSharp() =>
        nfluent
        .AddTestReference()
        .AddPaths("AssertionsShouldBeComplete.NFluent.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_NFluent_CSharpLatest() =>
        nfluent
        .WithOptions(LanguageOptions.CSharpLatest)
        .AddTestReference()
        .AddPaths("AssertionsShouldBeComplete.NFluent.Latest.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_NSubstitute_CS() =>
        nsubstitute
        .AddPaths("AssertionsShouldBeComplete.NSubstitute.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_AllFrameworks_CS() =>
        allFrameworks
        .AddPaths("AssertionsShouldBeComplete.AllFrameworks.cs")
        .Verify();
}
