/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
public class PropertyWriteOnlyTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.PropertyWriteOnly>();

    [TestMethod]
    public void PropertyWriteOnly_CS() =>
        builderCS.AddPaths("PropertyWriteOnly.cs").Verify();

#if NET
    [TestMethod]
    public void PropertyWriteOnly_CS_Latest() =>
        builderCS
            .AddPaths("PropertyWriteOnly.Latest.cs")
            .AddPaths("PropertyWriteOnly.Latest.Partial.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
#endif

    [TestMethod]
    public void PropertyWriteOnly_VB() =>
        new VerifierBuilder<VB.PropertyWriteOnly>().AddPaths("PropertyWriteOnly.vb").Verify();
}
