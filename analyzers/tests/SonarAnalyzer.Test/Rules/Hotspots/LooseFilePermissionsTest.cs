/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
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
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Windows_CSharp11() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.CSharp11.cs")
            .WithTopLevelStatements()
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

    [TestMethod]
    public void LooseFilePermissions_Windows_CSharp12() =>
        builderCS.AddPaths("LooseFilePermissions.Windows.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
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
