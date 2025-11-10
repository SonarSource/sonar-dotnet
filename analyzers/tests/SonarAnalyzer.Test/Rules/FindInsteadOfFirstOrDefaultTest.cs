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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class FindInsteadOfFirstOrDefaultTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.FindInsteadOfFirstOrDefault>().WithConcurrentAnalysis(false);
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.FindInsteadOfFirstOrDefault>();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.cs").AddReferences(GetReferencesEntityFrameworkNetCore("7.0.5")).Verify();

    internal static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetCore(string entityFrameworkVersion) =>
        Enumerable.Empty<MetadataReference>()
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore(entityFrameworkVersion))
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion));

#if NET

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Immutable_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.Immutable.cs").AddReferences(MetadataReferenceFacade.SystemCollections).Verify();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Net_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.Net.cs").Verify();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Net_VB() =>
        builderVB.AddPaths("FindInsteadOfFirstOrDefault.Net.vb").Verify();

#endif

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_Array_CS() =>
        builderCS.AddPaths("FindInsteadOfFirstOrDefault.Array.cs").Verify();

    [TestMethod]
    public void FindInsteadOfFirstOrDefault_VB() =>
        builderVB.AddPaths("FindInsteadOfFirstOrDefault.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();
}
