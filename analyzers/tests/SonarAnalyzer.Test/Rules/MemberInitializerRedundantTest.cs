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
public class MemberInitializerRedundantTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MemberInitializerRedundant>();
    private readonly VerifierBuilder builderSonarCfg = new VerifierBuilder().AddAnalyzer(() => new MemberInitializerRedundant(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg));

    [TestMethod]
    public void MemberInitializerRedundant_RoslynCfg() =>
        builder.AddPaths(@"MemberInitializerRedundant.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void MemberInitializerRedundant_RoslynCfg_FlowCaptureOperationNotSupported() =>
        builder.AddPaths(@"MemberInitializerRedundant.RoslynCfg.FlowCaptureBug.cs").WithOptions(LanguageOptions.FromCSharp8).VerifyNoIssues();

    [TestMethod]
    public void MemberInitializerRedundant_SonarCfg() =>
        builderSonarCfg.AddPaths(@"MemberInitializerRedundant.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void MemberInitializerRedundant_CodeFix() =>
        builder
            .WithCodeFix<MemberInitializedToDefaultCodeFix>()
            .AddPaths("MemberInitializerRedundant.cs")
            .WithCodeFixedPaths("MemberInitializerRedundant.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void MemberInitializerRedundant_CSharpLatest() =>
        builder.AddPaths("MemberInitializerRedundant.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void MemberInitializerRedundant_CSharpLatest_CodeFix() =>
        builder
            .WithCodeFix<MemberInitializedToDefaultCodeFix>()
            .AddPaths("MemberInitializerRedundant.Latest.cs")
            .WithCodeFixedPaths("MemberInitializerRedundant.Latest.Fixed.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .VerifyCodeFix();
}
