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
public class DateAndTimeShouldNotBeUsedAsTypeForPrimaryKeyTest
{
    private readonly VerifierBuilder verifierCS = CreateVerifier<CS.DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey>();
    private readonly VerifierBuilder verifierVB = CreateVerifier<VB.DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey>();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_CS() =>
        verifierCS.AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.cs").Verify();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_VB() =>
        verifierVB.AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.vb").Verify();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_NoReferenceToEntityFramework_CS() =>
        new VerifierBuilder<CS.DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey>()
            .AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.NoReferenceToEntityFramework.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_NoReferenceToEntityFramework_VB() =>
        new VerifierBuilder<VB.DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey>()
            .AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.NoReferenceToEntityFramework.vb")
            .VerifyNoIssues();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_CSharp9() =>
        verifierCS
            .AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .VerifyNoIssues();

#if NET

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_EntityFrameworkCore_CS() =>
        verifierCS.AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.EntityFrameworkCore.cs").Verify();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_EntityFrameworkCore_VB() =>
        verifierVB.AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.EntityFrameworkCore.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_FluentApi_CS() =>
        verifierCS.AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.FluentApi.cs").VerifyNoIssues();

    [TestMethod]
    public void DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey_FluentApi_VB() =>
        verifierVB.AddPaths("DateAndTimeShouldNotBeUsedAsTypeForPrimaryKey.FluentApi.vb").VerifyNoIssues();

#endif

    private static VerifierBuilder CreateVerifier<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        new VerifierBuilder<TAnalyzer>()
            .AddReferences(NuGetMetadataReference.SystemComponentModelAnnotations())

#if NET

            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore("7.0.0"))
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreAbstractions("7.0.0"));

#else

            .AddReferences(NuGetMetadataReference.MicrosoftEntityFramework("6.0.0"));

#endif

}
