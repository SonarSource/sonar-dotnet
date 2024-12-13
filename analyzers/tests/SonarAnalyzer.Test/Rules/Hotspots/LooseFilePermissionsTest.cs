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

namespace SonarAnalyzer.Test.Rules.Hotspots;

[TestClass]
public class LooseFilePermissionsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled));
    private readonly VerifierBuilder builderVB = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new VB.LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void LooseFilePermissions_Windows_CS() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.cs").Verify();

    [TestMethod]
    public void LooseFilePermissions_Windows_VB() =>
        builderVB.AddPaths("LooseFilePermissions.Windows.vb").Verify();

#if NET

    [TestMethod]
    public void LooseFilePermissions_Windows_CSharp9() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Windows_CSharp10() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.CSharp10.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Windows_CSharp11() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.CSharp11.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp11)
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Windows_CSharp12() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.CSharp12.cs")
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Unix_CS() =>
        builderCS.AddPaths("LooseFilePermissions.Unix.cs")
            .AddReferences(NuGetMetadataReference.MonoPosixNetStandard())
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Unix_VB() =>
        builderVB.AddPaths("LooseFilePermissions.Unix.vb")
            .AddReferences(NuGetMetadataReference.MonoPosixNetStandard())
            .Verify();

#endif

}
