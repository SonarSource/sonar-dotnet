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
public class HardcodedIpAddressTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.HardcodedIpAddress(AnalyzerConfiguration.AlwaysEnabled));
    private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.HardcodedIpAddress(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void HardcodedIpAddress_CS() =>
        builderCS.AddPaths(@"Hotspots\HardcodedIpAddress.cs").Verify();

#if NET

    [TestMethod]
    public void HardcodedIpAddress_CS_Latest() =>
        builderCS.AddPaths(@"Hotspots\HardcodedIpAddress.Latest.cs")
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .Verify();

#endif

    [TestMethod]
    public void HardcodedIpAddress_VB() =>
        builderVB.AddPaths(@"Hotspots\HardcodedIpAddress.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();
}
