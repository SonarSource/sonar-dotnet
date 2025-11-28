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
public class MethodOverrideChangedDefaultValueTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MethodOverrideChangedDefaultValue>();

    [TestMethod]
    public void MethodOverrideChangedDefaultValue() =>
        builder.AddPaths("MethodOverrideChangedDefaultValue.cs")
            .AddReferences(MetadataReferenceFacade.NetStandard21)
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void MethodOverrideChangedDefaultValue_CSharpLatest() =>
        builder.AddPaths("MethodOverrideChangedDefaultValue.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void MethodOverrideChangedDefaultValue_CSharpLatest_CodeFix() =>
        builder.AddPaths("MethodOverrideChangedDefaultValue.Latest.cs")
            .WithCodeFix<MethodOverrideChangedDefaultValueCodeFix>()
            .WithCodeFixedPaths("MethodOverrideChangedDefaultValue.Latest.Fixed.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .VerifyCodeFix();

    [TestMethod]
    public void MethodOverrideChangedDefaultValue_CodeFix_Synchronize() =>
        builder.AddPaths("MethodOverrideChangedDefaultValue.cs")
            .WithCodeFix<MethodOverrideChangedDefaultValueCodeFix>()
            .WithCodeFixedPaths("MethodOverrideChangedDefaultValue.Synchronize.Fixed.cs", "MethodOverrideChangedDefaultValue.Synchronize.Fixed.Batch.cs")
            .WithCodeFixTitle(MethodOverrideChangedDefaultValueCodeFix.TitleGeneral)
            .VerifyCodeFix();

    [TestMethod]
    public void MethodOverrideChangedDefaultValue_CodeFix_Remove() =>
        builder.AddPaths("MethodOverrideChangedDefaultValue.cs")
            .WithCodeFix<MethodOverrideChangedDefaultValueCodeFix>()
            .WithCodeFixedPaths("MethodOverrideChangedDefaultValue.Remove.Fixed.cs")
            .WithCodeFixTitle(MethodOverrideChangedDefaultValueCodeFix.TitleExplicitInterface)
            .VerifyCodeFix();
}
