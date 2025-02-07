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

using SonarAnalyzer.Test.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestsShouldNotUseThreadSleepTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.TestsShouldNotUseThreadSleep>()
        .AddReferences(NuGetMetadataReference.NUnit(TestConstants.NuGetLatestVersion))
        .AddReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion))
        .AddReferences(NuGetMetadataReference.XunitFramework(XUnitVersions.Ver253));

    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.TestsShouldNotUseThreadSleep>()
        .AddReferences(NuGetMetadataReference.NUnit(TestConstants.NuGetLatestVersion))
        .AddReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion))
        .AddReferences(NuGetMetadataReference.XunitFramework(XUnitVersions.Ver253));

    [TestMethod]
    public void TestsShouldNotUseThreadSleep_CS() =>
        builderCS.AddPaths("TestsShouldNotUseThreadSleep.cs").Verify();

    [TestMethod]
    public void TestsShouldNotUseThreadSleep_VB() =>
        builderVB.AddPaths("TestsShouldNotUseThreadSleep.vb").Verify();
}
