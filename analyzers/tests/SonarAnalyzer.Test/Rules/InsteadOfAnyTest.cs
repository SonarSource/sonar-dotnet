/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class InsteadOfAnyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.InsteadOfAny>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InsteadOfAny>();

    [TestMethod]
    public void InsteadOfAny_CS() =>
        builderCS.AddPaths("InsteadOfAny.cs").Verify();

#if NET

    [TestMethod]
    public void InsteadOfAny_TopLevelStatements() =>
        builderCS.AddPaths("InsteadOfAny.CSharp9.cs")
                 .WithTopLevelStatements()
                 .AddReferences(MetadataReferenceFacade.SystemCollections)
                 .Verify();
#endif

    [TestMethod]
    public void InsteadOfAny_EntityFramework() =>
        builderCS.AddPaths("InsteadOfAny.EntityFramework.Core.cs")
                 .WithOptions(LanguageOptions.FromCSharp8)
                 .AddReferences(GetReferencesEntityFrameworkNetCore())
                 .Verify();

#if NETFRAMEWORK

    [TestMethod]
    public void InsteadOfAny_EntityFramework_Framework() =>
        builderCS.AddPaths("InsteadOfAny.EntityFramework.Framework.cs")
                 .AddReferences(GetReferencesEntityFrameworkNetFramework())
                 .VerifyNoIssues();

#endif

    [TestMethod]
    public void ExistsInsteadOfAny_VB() =>
        builderVB.AddPaths("InsteadOfAny.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();

    [TestMethod]
    public void InsteadOfAny_EntityFramework_VB() =>
        builderVB.AddPaths("InsteadOfAny.EntityFramework.Core.vb")
            .AddReferences(GetReferencesEntityFrameworkNetCore())
            .Verify();

    private static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetCore() =>
        Enumerable.Empty<MetadataReference>()
        .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore("2.2.6"))
        .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational("2.2.6"))
        .Concat(NuGetMetadataReference.SystemComponentModelTypeConverter());

    private static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNetFramework() =>
        Enumerable.Empty<MetadataReference>()
        .Concat(NuGetMetadataReference.EntityFramework());

}
