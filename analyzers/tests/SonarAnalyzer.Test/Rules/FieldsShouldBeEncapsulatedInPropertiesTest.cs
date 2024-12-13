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
public class FieldsShouldBeEncapsulatedInPropertiesTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<FieldsShouldBeEncapsulatedInProperties>();

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.cs").Verify();

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties_Unity3D_Ignored() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.Unity3D.cs")
            // Concurrent analysis puts fake Unity3D class into the Concurrent namespace
            .WithConcurrentAnalysis(false)
            .Verify();

#if NET

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties_CSharp9() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void FieldsShouldBeEncapsulatedInProperties_CSharp12() =>
        builder.AddPaths("FieldsShouldBeEncapsulatedInProperties.CSharp12.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

#endif

}
