/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.Rules;

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

    [TestMethod]
    public void AssertionsShouldBeComplete_FluentAssertions_CSharp7() =>
        fluentAssertions
        .WithOptions(ParseOptionsHelper.OnlyCSharp7)
        .AddPaths("AssertionsShouldBeComplete.FluentAssertions.CSharp7.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_FluentAssertions_CSharp8() =>
        fluentAssertions
        .WithOptions(ParseOptionsHelper.FromCSharp8)
        .AddPaths("AssertionsShouldBeComplete.FluentAssertions.CSharp8.cs")
        .WithConcurrentAnalysis(false)
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_NFluent_CSharp() =>
        nfluent
        .AddTestReference()
        .AddPaths("AssertionsShouldBeComplete.NFluent.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_NFluent_CSharp11() =>
        nfluent
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddTestReference()
        .AddPaths("AssertionsShouldBeComplete.NFluent.CSharp11.cs")
        .Verify();

    [TestMethod]
    public void AssertionsShouldBeComplete_NSubstitute_CS() =>
        nsubstitute
        .AddPaths("AssertionsShouldBeComplete.NSubstitute.cs")
        .Verify();
}
