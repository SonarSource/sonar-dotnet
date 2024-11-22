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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class StringLiteralShouldNotBeDuplicatedTest
{
#if NET
    private static readonly ImmutableArray<MetadataReference> DapperReferences = [
        CoreMetadataReference.SystemDataCommon,
        CoreMetadataReference.SystemComponentModelPrimitives,
        ..NuGetMetadataReference.Dapper(),
        ..NuGetMetadataReference.SystemDataSqlClient()
    ];
#endif

    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.StringLiteralShouldNotBeDuplicated>();

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_CS() =>
        builderCS.AddPaths("StringLiteralShouldNotBeDuplicated.cs").Verify();

#if NET

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_CS_Latest() =>
        builderCS.AddPaths("StringLiteralShouldNotBeDuplicated.Latest.cs")
            .WithTopLevelStatements()
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .Verify();

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_CS_Dapper() =>
        builderCS.AddPaths("StringLiteralShouldNotBeDuplicated.Dapper.cs")
            .AddReferences(DapperReferences)
            .Verify();

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_VB_Dapper() =>
        new VerifierBuilder<VB.StringLiteralShouldNotBeDuplicated>()
            .AddPaths("StringLiteralShouldNotBeDuplicated.Dapper.vb")
            .AddReferences(DapperReferences)
            .Verify();

#endif

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_Attributes_CS() =>
        new VerifierBuilder().AddAnalyzer(() => new CS.StringLiteralShouldNotBeDuplicated { Threshold = 2 })
            .AddPaths("StringLiteralShouldNotBeDuplicated_Attributes.cs")
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_VB() =>
        new VerifierBuilder<VB.StringLiteralShouldNotBeDuplicated>()
            .AddPaths("StringLiteralShouldNotBeDuplicated.vb")
            .Verify();

    [TestMethod]
    public void StringLiteralShouldNotBeDuplicated_Attributes_VB() =>
        new VerifierBuilder().AddAnalyzer(() => new VB.StringLiteralShouldNotBeDuplicated() { Threshold = 2 })
            .AddPaths("StringLiteralShouldNotBeDuplicated_Attributes.vb")
            .WithConcurrentAnalysis(false)
            .Verify();
}
